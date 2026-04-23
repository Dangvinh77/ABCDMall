using System.Data;
using System.Data.Common;
using System.Text.Json;
using ABCDMall.Modules.Movies.Application.Services.Bookings;
using ABCDMall.Modules.Movies.Application.Services.Showtimes;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking;
using ABCDMall.Modules.Movies.Infrastructure.Services.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ABCDMall.Modules.Movies.Infrastructure.Repositories.Bookings;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly MoviesBookingDbContext _bookingDbContext;
    private readonly IShowtimeBookingPolicy _showtimeBookingPolicy;

    public PaymentRepository(
        MoviesBookingDbContext bookingDbContext,
        IShowtimeBookingPolicy showtimeBookingPolicy)
    {
        _bookingDbContext = bookingDbContext;
        _showtimeBookingPolicy = showtimeBookingPolicy;
    }

    public async Task<PaymentProcessingResult> ApplyPaymentResultAsync(
        Guid bookingId,
        PaymentProvider provider,
        string providerTransactionId,
        PaymentStatus status,
        decimal amount,
        string currency,
        string? rawPayload,
        string? failureReason,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _bookingDbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var booking = await _bookingDbContext.Bookings
            .Include(x => x.Items)
            .Include(x => x.Payments)
            .Include(x => x.Tickets)
            .FirstOrDefaultAsync(x => x.Id == bookingId, cancellationToken);

        if (booking is null)
        {
            throw new InvalidOperationException("Booking not found.");
        }

        var showtime = await ReadShowtimeAsync(transaction, booking.ShowtimeId, cancellationToken);
        if (showtime is null)
        {
            throw new InvalidOperationException("Showtime not found.");
        }
        _showtimeBookingPolicy.EnsureBookableForUser(showtime, utcNow);

        if (!decimal.Equals(decimal.Round(booking.GrandTotal, 2), decimal.Round(amount, 2)))
        {
            throw new InvalidOperationException("Payment amount does not match booking total.");
        }

        if (!string.Equals(booking.Currency, currency, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Payment currency does not match booking currency.");
        }

        var payment = await _bookingDbContext.Payments
            .FirstOrDefaultAsync(x => x.ProviderTransactionId == providerTransactionId, cancellationToken);

        if (payment is not null && payment.BookingId != booking.Id)
        {
            throw new InvalidOperationException("Provider transaction id has already been used for another booking.");
        }

        if (payment is not null && payment.Status == PaymentStatus.Succeeded)
        {
            if (booking.Status != BookingStatus.Confirmed && status == PaymentStatus.Succeeded)
            {
                await CompleteBookingAsync(booking, utcNow, cancellationToken);
                await IssueTicketsAndQueueEmailIfMissingAsync(booking, utcNow, cancellationToken);
                await CreateFeedbackRequestIfMissingAsync(booking, showtime, utcNow, cancellationToken);
                await _bookingDbContext.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return new PaymentProcessingResult
            {
                Payment = payment,
                Booking = booking
            };
        }

        if (booking.Status == BookingStatus.Confirmed)
        {
            throw new InvalidOperationException("Booking is already confirmed.");
        }

        if (payment is null)
        {
            payment = new Payment
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                CreatedAtUtc = utcNow
            };

            _bookingDbContext.Payments.Add(payment);
        }

        payment.Provider = provider;
        payment.ProviderTransactionId = providerTransactionId;
        payment.Status = status;
        payment.Amount = amount;
        payment.Currency = currency;
        payment.CallbackPayloadJson = rawPayload;
        payment.FailureReason = failureReason;
        payment.UpdatedAtUtc = utcNow;
        payment.CompletedAtUtc = status == PaymentStatus.Succeeded ? utcNow : null;

        if (status == PaymentStatus.Succeeded)
        {
            await CompleteBookingAsync(booking, utcNow, cancellationToken);
            await IssueTicketsAndQueueEmailIfMissingAsync(booking, utcNow, cancellationToken);
            await CreateFeedbackRequestIfMissingAsync(booking, showtime, utcNow, cancellationToken);
        }

        await _bookingDbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new PaymentProcessingResult
        {
            Payment = payment,
            Booking = booking
        };
    }

    public async Task<PaymentProcessingResult?> GetStatusAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        var payment = await _bookingDbContext.Payments
            .AsNoTracking()
            .Include(x => x.Booking)
            .FirstOrDefaultAsync(x => x.Id == paymentId, cancellationToken);

        if (payment?.Booking is null)
        {
            return null;
        }

        return new PaymentProcessingResult
        {
            Payment = payment,
            Booking = payment.Booking
        };
    }

    private async Task CompleteBookingAsync(
        Bookingg booking,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        if (booking.Status == BookingStatus.Confirmed)
        {
            return;
        }

        if (booking.Status != BookingStatus.PendingPayment)
        {
            throw new InvalidOperationException($"Booking is already {booking.Status}.");
        }

        var seatInventoryIds = booking.Items
            .Where(item => string.Equals(item.ItemType, "Seat", StringComparison.OrdinalIgnoreCase) && item.SeatInventoryId.HasValue)
            .Select(item => item.SeatInventoryId!.Value)
            .Distinct()
            .ToArray();

        if (seatInventoryIds.Length == 0)
        {
            throw new InvalidOperationException("Booking does not have any linked seat holds.");
        }

        var holds = await _bookingDbContext.BookingHolds
            .Include(x => x.Seats)
            .Where(x => x.ShowtimeId == booking.ShowtimeId
                && x.Status == BookingHoldStatus.Active
                && x.Seats.Any(seat => seatInventoryIds.Contains(seat.SeatInventoryId)))
            .ToListAsync(cancellationToken);

        if (holds.Count == 0)
        {
            throw new InvalidOperationException("Booking hold not found.");
        }

        var coveredSeatInventoryIds = holds
            .SelectMany(hold => hold.Seats)
            .Select(seat => seat.SeatInventoryId)
            .Distinct()
            .ToHashSet();

        if (seatInventoryIds.Any(seatInventoryId => !coveredSeatInventoryIds.Contains(seatInventoryId)))
        {
            throw new InvalidOperationException("One or more booking holds are no longer active.");
        }

        if (holds.Any(hold => hold.ExpiresAtUtc <= utcNow))
        {
            foreach (var hold in holds.Where(hold => hold.ExpiresAtUtc <= utcNow))
            {
                hold.Status = BookingHoldStatus.Expired;
                hold.UpdatedAtUtc = utcNow;
            }
            throw new InvalidOperationException("Booking hold has expired.");
        }

        await MarkSeatsBookedAsync(
            booking.ShowtimeId,
            seatInventoryIds,
            utcNow,
            cancellationToken);

        booking.Status = BookingStatus.Confirmed;
        booking.UpdatedAtUtc = utcNow;

        foreach (var hold in holds)
        {
            hold.Status = BookingHoldStatus.Converted;
            hold.UpdatedAtUtc = utcNow;
        }
    }

    private async Task IssueTicketsAndQueueEmailIfMissingAsync(
        Bookingg booking,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        var seatItems = booking.Items
            .Where(x => string.Equals(x.ItemType, "Seat", StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.ItemCode)
            .ToArray();

        if (seatItems.Length == 0)
        {
            return;
        }

        var existingTicketItemIds = booking.Tickets
            .Where(x => x.BookingItemId.HasValue)
            .Select(x => x.BookingItemId!.Value)
            .ToHashSet();

        foreach (var item in seatItems.Where(item => !existingTicketItemIds.Contains(item.Id)))
        {
            var ticketCode = GenerateTicketCode(booking.BookingCode, item.ItemCode);
            var qrCodeContent = $"ABCDMALL|BOOKING:{booking.BookingCode}|TICKET:{ticketCode}|SEAT:{item.ItemCode}";

            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                BookingItemId = item.Id,
                SeatInventoryId = item.SeatInventoryId,
                TicketCode = ticketCode,
                SeatCode = item.ItemCode,
                QrCodeContent = qrCodeContent,
                DeliveryStatus = TicketDeliveryStatuses.EmailPending,
                IssuedAtUtc = utcNow,
                UpdatedAtUtc = utcNow
            };

            booking.Tickets.Add(ticket);
            _bookingDbContext.Tickets.Add(ticket);
        }

        var alreadyQueued = await _bookingDbContext.OutboxEvents.AnyAsync(
            x => x.EventType == TicketEmailOutboxEvent.EventType
                && x.PayloadJson.Contains(booking.Id.ToString()),
            cancellationToken);

        if (alreadyQueued)
        {
            return;
        }

        _bookingDbContext.OutboxEvents.Add(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            EventType = TicketEmailOutboxEvent.EventType,
            PayloadJson = JsonSerializer.Serialize(new { bookingId = booking.Id }),
            Status = "Pending",
            OccurredAtUtc = utcNow
        });
    }

    private async Task CreateFeedbackRequestIfMissingAsync(
        Bookingg booking,
        Showtime showtime,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        var exists = await _bookingDbContext.MovieFeedbackRequests
            .AnyAsync(
                x => x.BookingId == booking.Id && x.ShowtimeId == booking.ShowtimeId,
                cancellationToken);

        if (exists)
        {
            return;
        }

        var availableAtUtc = showtime.EndAtUtc ?? showtime.StartAtUtc;

        _bookingDbContext.MovieFeedbackRequests.Add(new MovieFeedbackRequest
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            MovieId = showtime.MovieId,
            ShowtimeId = booking.ShowtimeId,
            PurchaserEmail = booking.CustomerEmail,
            Status = MovieFeedbackRequestStatus.Pending,
            AvailableAtUtc = availableAtUtc,
            ExpiresAtUtc = availableAtUtc.AddHours(72),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow
        });
    }

    private async Task MarkSeatsBookedAsync(
        Guid showtimeId,
        IReadOnlyCollection<Guid> seatInventoryIds,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        var transaction = _bookingDbContext.Database.CurrentTransaction
            ?? throw new InvalidOperationException("A database transaction is required to book seats.");

        var rows = await ReadSeatStatusRowsAsync(
            transaction,
            showtimeId,
            seatInventoryIds,
            cancellationToken);

        if (rows.Count != seatInventoryIds.Count)
        {
            throw new InvalidOperationException("Some selected seats were not found for this showtime.");
        }

        var unavailableSeats = rows
            .Where(x => x.Status != SeatInventoryStatus.Available)
            .Select(x => x.SeatCode)
            .ToArray();

        if (unavailableSeats.Length > 0)
        {
            throw new InvalidOperationException($"Selected seats are no longer available: {string.Join(", ", unavailableSeats)}.");
        }

        var affectedRows = await UpdateSeatStatusRowsAsync(
            transaction,
            showtimeId,
            seatInventoryIds,
            utcNow,
            cancellationToken);

        if (affectedRows != seatInventoryIds.Count)
        {
            throw new InvalidOperationException("Selected seats could not be booked.");
        }
    }

    private async Task<Showtime?> ReadShowtimeAsync(
        IDbContextTransaction transaction,
        Guid showtimeId,
        CancellationToken cancellationToken)
    {
        await using var command = _bookingDbContext.Database.GetDbConnection().CreateCommand();
        command.Transaction = transaction.GetDbTransaction();
        command.CommandText = """
            SELECT [Id], [MovieId], [CinemaId], [HallId], [BusinessDate], [StartAtUtc], [EndAtUtc], [Language], [BasePrice], [Status]
            FROM [movies].[Showtimes]
            WHERE [Id] = @showtimeId
            """;
        AddParameter(command, "@showtimeId", showtimeId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new Showtime
        {
            Id = reader.GetGuid(0),
            MovieId = reader.GetGuid(1),
            CinemaId = reader.GetGuid(2),
            HallId = reader.GetGuid(3),
            BusinessDate = DateOnly.FromDateTime(reader.GetDateTime(4)),
            StartAtUtc = reader.GetDateTime(5),
            EndAtUtc = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
            Language = (LanguageType)reader.GetInt32(7),
            BasePrice = reader.GetDecimal(8),
            Status = (ShowtimeStatus)reader.GetInt32(9)
        };
    }

    private async Task<IReadOnlyCollection<SeatStatusRow>> ReadSeatStatusRowsAsync(
        IDbContextTransaction transaction,
        Guid showtimeId,
        IReadOnlyCollection<Guid> seatInventoryIds,
        CancellationToken cancellationToken)
    {
        await using var command = CreateSeatCommand(
            transaction,
            showtimeId,
            seatInventoryIds,
            """
            SELECT [Id], [SeatCode], [Status]
            FROM [movies].[ShowtimeSeatInventory]
            WHERE [ShowtimeId] = @showtimeId
              AND [Id] IN ({0})
            """);

        var rows = new List<SeatStatusRow>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new SeatStatusRow(
                reader.GetGuid(0),
                reader.GetString(1),
                (SeatInventoryStatus)reader.GetInt32(2)));
        }

        return rows;
    }

    private async Task<int> UpdateSeatStatusRowsAsync(
        IDbContextTransaction transaction,
        Guid showtimeId,
        IReadOnlyCollection<Guid> seatInventoryIds,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        await using var command = CreateSeatCommand(
            transaction,
            showtimeId,
            seatInventoryIds,
            """
            UPDATE [movies].[ShowtimeSeatInventory]
            SET [Status] = @bookedStatus,
                [UpdatedAtUtc] = @updatedAtUtc
            WHERE [ShowtimeId] = @showtimeId
              AND [Status] = @availableStatus
              AND [Id] IN ({0})
            """);

        AddParameter(command, "@bookedStatus", (int)SeatInventoryStatus.Booked);
        AddParameter(command, "@availableStatus", (int)SeatInventoryStatus.Available);
        AddParameter(command, "@updatedAtUtc", utcNow);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private DbCommand CreateSeatCommand(
        IDbContextTransaction transaction,
        Guid showtimeId,
        IReadOnlyCollection<Guid> seatInventoryIds,
        string sqlTemplate)
    {
        var command = _bookingDbContext.Database.GetDbConnection().CreateCommand();
        command.Transaction = transaction.GetDbTransaction();
        AddParameter(command, "@showtimeId", showtimeId);

        var parameterNames = seatInventoryIds
            .Select((seatInventoryId, index) =>
            {
                var parameterName = $"@seat{index}";
                AddParameter(command, parameterName, seatInventoryId);
                return parameterName;
            })
            .ToArray();

        command.CommandText = string.Format(sqlTemplate, string.Join(", ", parameterNames));
        return command;
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    private static string GenerateTicketCode(string bookingCode, string seatCode)
    {
        var bookingSuffix = bookingCode.Length <= 8 ? bookingCode : bookingCode[^8..];
        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        return $"TCK-{bookingSuffix}-{seatCode}-{suffix}";
    }

    private sealed record SeatStatusRow(Guid Id, string SeatCode, SeatInventoryStatus Status);
}

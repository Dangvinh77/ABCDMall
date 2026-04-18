using System.Data;
using System.Data.Common;
using ABCDMall.Modules.Movies.Application.Services.Bookings;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ABCDMall.Modules.Movies.Infrastructure.Repositories.Bookings;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly MoviesBookingDbContext _bookingDbContext;

    public PaymentRepository(MoviesBookingDbContext bookingDbContext)
    {
        _bookingDbContext = bookingDbContext;
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
            .FirstOrDefaultAsync(x => x.Id == bookingId, cancellationToken);

        if (booking is null)
        {
            throw new InvalidOperationException("Booking not found.");
        }

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

        if (!booking.BookingHoldId.HasValue)
        {
            throw new InvalidOperationException("Booking does not have a linked hold.");
        }

        var hold = await _bookingDbContext.BookingHolds
            .Include(x => x.Seats)
            .FirstOrDefaultAsync(x => x.Id == booking.BookingHoldId.Value, cancellationToken);

        if (hold is null)
        {
            throw new InvalidOperationException("Booking hold not found.");
        }

        if (hold.Status != BookingHoldStatus.Active)
        {
            throw new InvalidOperationException($"Booking hold is already {hold.Status}.");
        }

        if (hold.ExpiresAtUtc <= utcNow)
        {
            hold.Status = BookingHoldStatus.Expired;
            hold.UpdatedAtUtc = utcNow;
            throw new InvalidOperationException("Booking hold has expired.");
        }

        var seatInventoryIds = hold.Seats
            .Select(x => x.SeatInventoryId)
            .Distinct()
            .ToArray();

        await MarkSeatsBookedAsync(
            booking.ShowtimeId,
            seatInventoryIds,
            utcNow,
            cancellationToken);

        booking.Status = BookingStatus.Confirmed;
        booking.UpdatedAtUtc = utcNow;

        hold.Status = BookingHoldStatus.Converted;
        hold.UpdatedAtUtc = utcNow;
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

    private sealed record SeatStatusRow(Guid Id, string SeatCode, SeatInventoryStatus Status);
}

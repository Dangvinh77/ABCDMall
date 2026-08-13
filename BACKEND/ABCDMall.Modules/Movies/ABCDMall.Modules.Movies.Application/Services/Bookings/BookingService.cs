using ABCDMall.Modules.Movies.Application.DTOs.Bookings;
using ABCDMall.Modules.Movies.Application.Services.Showtimes;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;
using System.Text.Json;

namespace ABCDMall.Modules.Movies.Application.Services.Bookings;

public sealed class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IShowtimeRepository _showtimeRepository;
    private readonly IShowtimeBookingPolicy _showtimeBookingPolicy;

    public BookingService(
        IBookingRepository bookingRepository,
        IShowtimeRepository showtimeRepository,
        IShowtimeBookingPolicy showtimeBookingPolicy)
    {
        _bookingRepository = bookingRepository;
        _showtimeRepository = showtimeRepository;
        _showtimeBookingPolicy = showtimeBookingPolicy;
    }

    public async Task<CreateBookingResponseDto> CreateAsync(
        CreateBookingRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var holdIds = request.HoldIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .OrderBy(id => id)
            .ToArray();

        if (holdIds.Length == 0)
        {
            throw new InvalidOperationException("At least one booking hold is required.");
        }

        var existingBooking = await _bookingRepository.GetByCombinedHoldIdsAsync(holdIds, cancellationToken);

        if (existingBooking is not null)
        {
            return MapCreateResponse(existingBooking, holdIds);
        }

        var holds = await _bookingRepository.GetHoldsForBookingAsync(holdIds, cancellationToken);
        if (holds.Count != holdIds.Length)
        {
            throw new InvalidOperationException("One or more booking holds were not found.");
        }

        if (holds.Any(hold => hold.Status != BookingHoldStatus.Active))
        {
            throw new InvalidOperationException("One or more booking holds are no longer active.");
        }

        if (holds.Any(hold => hold.ExpiresAtUtc <= now))
        {
            throw new InvalidOperationException("One or more booking holds have expired.");
        }

        var showtimeId = holds
            .Select(hold => hold.ShowtimeId)
            .Distinct()
            .SingleOrDefault();

        if (showtimeId == Guid.Empty)
        {
            throw new InvalidOperationException("Booking holds must belong to the same showtime.");
        }

        var showtime = await _showtimeRepository.GetShowtimeByIdAsync(showtimeId, cancellationToken);
        if (showtime is null)
        {
            throw new InvalidOperationException("Showtime not found.");
        }
        _showtimeBookingPolicy.EnsureBookableForUser(showtime, now);

        var guestCustomer = await _bookingRepository.FindGuestCustomerAsync(
            request.CustomerEmail,
            request.CustomerPhoneNumber,
            cancellationToken);

        var newGuestCustomer = guestCustomer is null
            ? new GuestCustomer
            {
                Id = Guid.NewGuid(),
                FullName = request.CustomerName.Trim(),
                Email = request.CustomerEmail.Trim(),
                PhoneNumber = request.CustomerPhoneNumber.Trim(),
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            }
            : null;

        var booking = new Bookingg
        {
            Id = Guid.NewGuid(),
            BookingCode = GenerateBookingCode(now),
            ShowtimeId = showtimeId,
            GuestCustomerId = guestCustomer?.Id ?? newGuestCustomer!.Id,
            BookingHoldId = holdIds[0],
            PromotionId = holds.Select(hold => hold.PromotionId).FirstOrDefault(id => id.HasValue),
            Status = BookingStatus.PendingPayment,
            CustomerName = request.CustomerName.Trim(),
            CustomerEmail = request.CustomerEmail.Trim(),
            CustomerPhoneNumber = request.CustomerPhoneNumber.Trim(),
            SeatSubtotal = holds.Sum(hold => hold.SeatSubtotal),
            ComboSubtotal = holds.Sum(hold => hold.ComboSubtotal),
            ServiceFee = holds.Sum(hold => hold.ServiceFee),
            DiscountAmount = holds.Sum(hold => hold.DiscountAmount),
            GrandTotal = holds.Sum(hold => hold.GrandTotal),
            Currency = "VND",
            PromotionSnapshotJson = holds.Select(hold => hold.PromotionSnapshotJson).FirstOrDefault(snapshot => !string.IsNullOrWhiteSpace(snapshot)),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        foreach (var seat in holds.SelectMany(hold => hold.Seats).OrderBy(x => x.SeatCode))
        {
            booking.Items.Add(new BookingItem
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                ItemType = "Seat",
                ItemCode = seat.SeatCode,
                Description = $"Seat {seat.SeatCode}",
                SeatInventoryId = seat.SeatInventoryId,
                Quantity = 1,
                UnitPrice = seat.UnitPrice,
                LineTotal = seat.UnitPrice
            });
        }

        foreach (var combo in holds.SelectMany(hold => ReadComboSnapshots(hold.ComboSnapshotJson)))
        {
            booking.Items.Add(new BookingItem
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                ItemType = "Combo",
                ItemCode = combo.ComboCode,
                Description = combo.ComboName,
                Quantity = combo.Quantity,
                UnitPrice = combo.UnitPrice,
                LineTotal = combo.LineTotal
            });
        }

        var totalServiceFee = holds.Sum(hold => hold.ServiceFee);
        if (totalServiceFee > 0)
        {
            booking.Items.Add(new BookingItem
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                ItemType = "Fee",
                ItemCode = "SERVICE_FEE",
                Description = "Service fee",
                Quantity = 1,
                UnitPrice = totalServiceFee,
                LineTotal = totalServiceFee
            });
        }

        var totalDiscount = holds.Sum(hold => hold.DiscountAmount);
        if (totalDiscount > 0)
        {
            booking.Items.Add(new BookingItem
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                ItemType = "Discount",
                ItemCode = "PROMOTION_DISCOUNT",
                Description = "Promotion discount",
                Quantity = 1,
                UnitPrice = -totalDiscount,
                LineTotal = -totalDiscount
            });
        }

        var created = await _bookingRepository.AddPendingBookingAsync(
            booking,
            newGuestCustomer,
            holdIds,
            now,
            cancellationToken);

        return MapCreateResponse(created, holdIds);
    }

    public async Task<BookingDetailResponseDto?> GetByCodeAsync(
        string bookingCode,
        CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetByCodeAsync(bookingCode, cancellationToken);
        return booking is null ? null : MapDetail(booking);
    }

    private static IReadOnlyCollection<BookingHoldComboSnapshotDto> ReadComboSnapshots(string? comboSnapshotJson)
    {
        if (string.IsNullOrWhiteSpace(comboSnapshotJson))
        {
            return Array.Empty<BookingHoldComboSnapshotDto>();
        }

        return JsonSerializer.Deserialize<IReadOnlyCollection<BookingHoldComboSnapshotDto>>(comboSnapshotJson)
            ?? Array.Empty<BookingHoldComboSnapshotDto>();
    }

    private static CreateBookingResponseDto MapCreateResponse(Bookingg booking, IReadOnlyCollection<Guid> holdIds)
    {
        return new CreateBookingResponseDto
        {
            BookingId = booking.Id,
            BookingCode = booking.BookingCode,
            ShowtimeId = booking.ShowtimeId,
            HoldIds = holdIds.ToArray(),
            Status = booking.Status.ToString(),
            GrandTotal = booking.GrandTotal,
            Currency = booking.Currency,
            PaymentRequired = booking.Status == BookingStatus.PendingPayment
        };
    }

    private static BookingDetailResponseDto MapDetail(Bookingg booking)
    {
        return new BookingDetailResponseDto
        {
            BookingId = booking.Id,
            BookingCode = booking.BookingCode,
            ShowtimeId = booking.ShowtimeId,
            HoldId = booking.BookingHoldId,
            GuestCustomerId = booking.GuestCustomerId,
            PromotionId = booking.PromotionId,
            Status = booking.Status.ToString(),
            CustomerName = booking.CustomerName,
            CustomerEmail = booking.CustomerEmail,
            CustomerPhoneNumber = booking.CustomerPhoneNumber,
            SeatSubtotal = booking.SeatSubtotal,
            ComboSubtotal = booking.ComboSubtotal,
            ServiceFee = booking.ServiceFee,
            DiscountAmount = booking.DiscountAmount,
            GrandTotal = booking.GrandTotal,
            Currency = booking.Currency,
            PromotionSnapshotJson = booking.PromotionSnapshotJson,
            CreatedAtUtc = booking.CreatedAtUtc,
            UpdatedAtUtc = booking.UpdatedAtUtc,
            Items = booking.Items.Select(item => new BookingItemResponseDto
            {
                Id = item.Id,
                ItemType = item.ItemType,
                ItemCode = item.ItemCode,
                Description = item.Description,
                SeatInventoryId = item.SeatInventoryId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = item.LineTotal
            }).ToArray()
        };
    }

    private static string GenerateBookingCode(DateTime utcNow)
    {
        return $"BK-{utcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..32];
    }
}

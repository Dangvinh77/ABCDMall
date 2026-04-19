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
        var existingBooking = await _bookingRepository.GetByHoldIdAsync(request.HoldId, cancellationToken);

        if (existingBooking is not null)
        {
            return MapCreateResponse(existingBooking);
        }

        var hold = await _bookingRepository.GetHoldForBookingAsync(request.HoldId, cancellationToken);
        if (hold is null)
        {
            throw new InvalidOperationException("Booking hold not found.");
        }

        if (hold.Status != BookingHoldStatus.Active)
        {
            throw new InvalidOperationException($"Booking hold is already {hold.Status}.");
        }

        if (hold.ExpiresAtUtc <= now)
        {
            throw new InvalidOperationException("Booking hold has expired.");
        }

        var showtime = await _showtimeRepository.GetShowtimeByIdAsync(hold.ShowtimeId, cancellationToken);
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
            ShowtimeId = hold.ShowtimeId,
            GuestCustomerId = guestCustomer?.Id ?? newGuestCustomer!.Id,
            BookingHoldId = hold.Id,
            PromotionId = hold.PromotionId,
            Status = BookingStatus.PendingPayment,
            CustomerName = request.CustomerName.Trim(),
            CustomerEmail = request.CustomerEmail.Trim(),
            CustomerPhoneNumber = request.CustomerPhoneNumber.Trim(),
            SeatSubtotal = hold.SeatSubtotal,
            ComboSubtotal = hold.ComboSubtotal,
            ServiceFee = hold.ServiceFee,
            DiscountAmount = hold.DiscountAmount,
            GrandTotal = hold.GrandTotal,
            Currency = "VND",
            PromotionSnapshotJson = hold.PromotionSnapshotJson,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        foreach (var seat in hold.Seats.OrderBy(x => x.SeatCode))
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

        foreach (var combo in ReadComboSnapshots(hold.ComboSnapshotJson))
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

        if (hold.ServiceFee > 0)
        {
            booking.Items.Add(new BookingItem
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                ItemType = "Fee",
                ItemCode = "SERVICE_FEE",
                Description = "Service fee",
                Quantity = 1,
                UnitPrice = hold.ServiceFee,
                LineTotal = hold.ServiceFee
            });
        }

        if (hold.DiscountAmount > 0)
        {
            booking.Items.Add(new BookingItem
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                ItemType = "Discount",
                ItemCode = "PROMOTION_DISCOUNT",
                Description = "Promotion discount",
                Quantity = 1,
                UnitPrice = -hold.DiscountAmount,
                LineTotal = -hold.DiscountAmount
            });
        }

        var created = await _bookingRepository.AddPendingBookingAsync(
            booking,
            newGuestCustomer,
            now,
            cancellationToken);

        return MapCreateResponse(created);
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

    private static CreateBookingResponseDto MapCreateResponse(Bookingg booking)
    {
        return new CreateBookingResponseDto
        {
            BookingId = booking.Id,
            BookingCode = booking.BookingCode,
            ShowtimeId = booking.ShowtimeId,
            HoldId = booking.BookingHoldId!.Value,
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

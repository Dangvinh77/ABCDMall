using ABCDMall.Modules.Movies.Application.DTOs.Bookings;
using ABCDMall.Modules.Movies.Application.Services.Showtimes;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;

namespace ABCDMall.Modules.Movies.Application.Services.Bookings;

public sealed class BookingHoldService : IBookingHoldService
{
    private static readonly TimeSpan HoldDuration = TimeSpan.FromMinutes(10);

    private readonly IShowtimeRepository _showtimeRepository;
    private readonly IBookingQuoteService _bookingQuoteService;
    private readonly IBookingHoldRepository _bookingHoldRepository;

    public BookingHoldService(
        IShowtimeRepository showtimeRepository,
        IBookingQuoteService bookingQuoteService,
        IBookingHoldRepository bookingHoldRepository)
    {
        _showtimeRepository = showtimeRepository;
        _bookingQuoteService = bookingQuoteService;
        _bookingHoldRepository = bookingHoldRepository;
    }

    public async Task<BookingHoldResponseDto> CreateAsync(
        CreateBookingHoldRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var showtime = await _showtimeRepository.GetShowtimeByIdAsync(request.ShowtimeId, cancellationToken);
        if (showtime is null)
        {
            throw new InvalidOperationException("Showtime not found.");
        }

        if (showtime.Status != ShowtimeStatus.Open)
        {
            throw new InvalidOperationException("Showtime is not open for booking.");
        }

        var seatMap = await _showtimeRepository.GetSeatMapByShowtimeIdAsync(request.ShowtimeId, cancellationToken);
        var selectedSeats = seatMap
            .Where(x => request.SeatInventoryIds.Contains(x.Id))
            .OrderBy(x => x.RowLabel)
            .ThenBy(x => x.ColumnNumber)
            .ToList();

        if (selectedSeats.Count != request.SeatInventoryIds.Count)
        {
            throw new InvalidOperationException("One or more selected seats were not found.");
        }

        var unavailableSeats = selectedSeats
            .Where(x => x.Status != SeatInventoryStatus.Available)
            .Select(x => x.SeatCode)
            .ToArray();

        if (unavailableSeats.Length > 0)
        {
            throw new InvalidOperationException($"Selected seats are not available: {string.Join(", ", unavailableSeats)}.");
        }

        ValidateCoupleSeatSelection(selectedSeats);

        var quote = await _bookingQuoteService.QuoteAsync(new BookingQuoteRequestDto
        {
            ShowtimeId = request.ShowtimeId,
            SeatInventoryIds = request.SeatInventoryIds,
            SnackCombos = request.SnackCombos,
            PromotionId = request.PromotionId,
            PaymentProvider = request.PaymentProvider,
            Birthday = request.Birthday,
            GuestCustomerId = request.GuestCustomerId
        }, cancellationToken);

        var now = DateTime.UtcNow;
        var hold = new BookingHold
        {
            Id = Guid.NewGuid(),
            HoldCode = GenerateHoldCode(now),
            ShowtimeId = request.ShowtimeId,
            Status = BookingHoldStatus.Active,
            ExpiresAtUtc = now.Add(HoldDuration),
            SessionId = request.SessionId,
            SeatSubtotal = quote.SeatSubtotal,
            ComboSubtotal = quote.ComboSubtotal,
            DiscountAmount = quote.DiscountTotal,
            GrandTotal = quote.GrandTotal,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            Seats = selectedSeats.Select(seat => new BookingHoldSeat
            {
                Id = Guid.NewGuid(),
                SeatInventoryId = seat.Id,
                SeatCode = seat.SeatCode,
                SeatType = seat.SeatType.ToString(),
                UnitPrice = seat.Price,
                CoupleGroupCode = seat.CoupleGroupCode
            }).ToList()
        };

        var created = await _bookingHoldRepository.AddAsync(hold, cancellationToken);
        return Map(created);
    }

    public async Task<BookingHoldResponseDto?> GetByIdAsync(Guid holdId, CancellationToken cancellationToken = default)
    {
        var hold = await _bookingHoldRepository.GetByIdAsync(holdId, cancellationToken);
        if (hold is null)
        {
            return null;
        }

        if (hold.Status == BookingHoldStatus.Active && hold.ExpiresAtUtc <= DateTime.UtcNow)
        {
            await _bookingHoldRepository.ExpireAsync(DateTime.UtcNow, cancellationToken);
            hold.Status = BookingHoldStatus.Expired;
        }

        return Map(hold);
    }

    public Task<bool> ReleaseAsync(Guid holdId, CancellationToken cancellationToken = default)
    {
        return _bookingHoldRepository.ReleaseAsync(holdId, DateTime.UtcNow, cancellationToken);
    }

    private static BookingHoldResponseDto Map(BookingHold hold)
    {
        var now = DateTime.UtcNow;
        var remainingSeconds = hold.ExpiresAtUtc <= now
            ? 0
            : (int)(hold.ExpiresAtUtc - now).TotalSeconds;

        return new BookingHoldResponseDto
        {
            HoldId = hold.Id,
            HoldCode = hold.HoldCode,
            ShowtimeId = hold.ShowtimeId,
            Status = hold.Status.ToString(),
            ExpiresAtUtc = hold.ExpiresAtUtc,
            RemainingSeconds = remainingSeconds,
            SeatSubtotal = hold.SeatSubtotal,
            ComboSubtotal = hold.ComboSubtotal,
            DiscountAmount = hold.DiscountAmount,
            GrandTotal = hold.GrandTotal,
            Seats = hold.Seats.Select(seat => new BookingHoldSeatResponseDto
            {
                SeatInventoryId = seat.SeatInventoryId,
                SeatCode = seat.SeatCode,
                SeatType = seat.SeatType,
                UnitPrice = seat.UnitPrice,
                CoupleGroupCode = seat.CoupleGroupCode
            }).ToList()
        };
    }

    private static void ValidateCoupleSeatSelection(IReadOnlyCollection<ShowtimeSeatInventory> seats)
    {
        var invalidCoupleGroups = seats
            .Where(x => x.SeatType == SeatType.Couple)
            .GroupBy(x => x.CoupleGroupCode)
            .Where(group => string.IsNullOrWhiteSpace(group.Key) || group.Count() != 2)
            .Select(group => group.Key ?? "(missing-group)")
            .ToArray();

        if (invalidCoupleGroups.Length > 0)
        {
            throw new InvalidOperationException("Couple seats must be selected as a full pair.");
        }
    }

    private static string GenerateHoldCode(DateTime utcNow)
    {
        return $"HOLD-{utcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..32];
    }
}

using ABCDMall.Modules.Movies.Application.Contracts;
using ABCDMall.Modules.Movies.Application.Services.Promotions;
using ABCDMall.Modules.Movies.Application.DTOs.Showtimes;
using ABCDMall.Modules.Movies.Application.Services.Bookings;
using ABCDMall.Modules.Movies.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ABCDMall.Modules.Movies.Application.Services.Showtimes;

public sealed class SeatMapQueryService : ISeatMapQueryService
{
    private readonly IShowtimeRepository _showtimeRepository;
    private readonly IBookingHoldRepository _bookingHoldRepository;
    private readonly IShowtimeBookingPolicy _showtimeBookingPolicy;
    private readonly IPromotionQueryService _promotionQueryService;
    private readonly ILogger<SeatMapQueryService> _logger;

    public SeatMapQueryService(
        IShowtimeRepository showtimeRepository,
        IBookingHoldRepository bookingHoldRepository,
        IShowtimeBookingPolicy showtimeBookingPolicy,
        IPromotionQueryService promotionQueryService,
        ILogger<SeatMapQueryService> logger)
    {
        _showtimeRepository = showtimeRepository;
        _bookingHoldRepository = bookingHoldRepository;
        _showtimeBookingPolicy = showtimeBookingPolicy;
        _promotionQueryService = promotionQueryService;
        _logger = logger;
    }

    public async Task<SeatMapResponseDto?> GetByShowtimeIdAsync(Guid showtimeId, CancellationToken cancellationToken = default)
    {
        var showtime = await _showtimeRepository.GetShowtimeByIdAsync(showtimeId, cancellationToken);
        if (showtime is null)
        {
            _logger.LogWarning("Seat map was requested for missing showtime {ShowtimeId}.", showtimeId);
            return null;
        }

        var seats = await _showtimeRepository.GetSeatMapByShowtimeIdAsync(showtimeId, cancellationToken);
        var bookingDecision = _showtimeBookingPolicy.EvaluateForUser(showtime, DateTime.UtcNow);
        var activeHoldSeatIds = await _bookingHoldRepository.GetActiveSeatInventoryIdsAsync(
            showtimeId,
            DateTime.UtcNow,
            cancellationToken);
        var promotions = await _promotionQueryService.GetPromotionsForShowtimeAsync(
            showtime.Id,
            showtime.BusinessDate,
            showtime.StartAtUtc,
            activeOnly: true,
            cancellationToken);

        _logger.LogInformation(
            "Fetched seat map for showtime {ShowtimeId} with {SeatCount} seats, {HeldSeatCount} active held seats, and {PromotionCount} contextual promotions.",
            showtimeId,
            seats.Count,
            activeHoldSeatIds.Count,
            promotions.Count);

        return new SeatMapResponseDto
        {
            ShowtimeId = showtime.Id,
            HallId = showtime.HallId,
            HallType = showtime.Hall is null ? string.Empty : MoviesContractValueMapper.ToContractValue(showtime.Hall.HallType),
            IsBookable = bookingDecision.IsBookable,
            BookingUnavailableReason = bookingDecision.UnavailableReason,
            Promotions = promotions,
            Seats = seats
                .OrderBy(seat => seat.RowLabel)
                .ThenBy(seat => seat.ColumnNumber)
                .Select(seat => new SeatMapSeatDto
                {
                    SeatInventoryId = seat.Id,
                    SeatCode = seat.SeatCode,
                    Row = seat.RowLabel,
                    Col = seat.ColumnNumber,
                    SeatType = seat.SeatType.ToString(),
                    Status = seat.Status == SeatInventoryStatus.Available && activeHoldSeatIds.Contains(seat.Id)
                        ? "Held"
                        : seat.Status.ToString(),
                    Price = seat.Price,
                    CoupleGroupCode = seat.CoupleGroupCode
                })
                .ToList()
        };
    }
}

using ABCDMall.Modules.Movies.Application.DTOs.Showtimes;
using ABCDMall.Modules.Movies.Application.Services.Bookings;
using ABCDMall.Modules.Movies.Domain.Enums;

namespace ABCDMall.Modules.Movies.Application.Services.Showtimes;

public sealed class SeatMapQueryService : ISeatMapQueryService
{
    private readonly IShowtimeRepository _showtimeRepository;
    private readonly IBookingHoldRepository _bookingHoldRepository;

    public SeatMapQueryService(
        IShowtimeRepository showtimeRepository,
        IBookingHoldRepository bookingHoldRepository)
    {
        _showtimeRepository = showtimeRepository;
        _bookingHoldRepository = bookingHoldRepository;
    }

    public async Task<SeatMapResponseDto?> GetByShowtimeIdAsync(Guid showtimeId, CancellationToken cancellationToken = default)
    {
        var showtime = await _showtimeRepository.GetShowtimeByIdAsync(showtimeId, cancellationToken);
        if (showtime is null)
        {
            return null;
        }

        var seats = await _showtimeRepository.GetSeatMapByShowtimeIdAsync(showtimeId, cancellationToken);
        var activeHoldSeatIds = await _bookingHoldRepository.GetActiveSeatInventoryIdsAsync(
            showtimeId,
            DateTime.UtcNow,
            cancellationToken);

        return new SeatMapResponseDto
        {
            ShowtimeId = showtime.Id,
            HallId = showtime.HallId,
            HallType = showtime.Hall?.HallType.ToString() ?? string.Empty,
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

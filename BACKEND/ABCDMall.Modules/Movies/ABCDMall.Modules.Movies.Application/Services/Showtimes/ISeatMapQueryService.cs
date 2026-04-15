using ABCDMall.Modules.Movies.Application.DTOs.Showtimes;

namespace ABCDMall.Modules.Movies.Application.Services.Showtimes;

public interface ISeatMapQueryService
{
    Task<SeatMapResponseDto?> GetByShowtimeIdAsync(Guid showtimeId, CancellationToken cancellationToken = default);
}

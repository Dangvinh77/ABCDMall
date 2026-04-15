using ABCDMall.Modules.Movies.Application.DTOs.Showtimes;

namespace ABCDMall.Modules.Movies.Application.Services.Showtimes;

public interface IShowtimeQueryService
{
    Task<IReadOnlyList<ShowtimeListItemDto>> GetListAsync(
        Guid? movieId = null,
        Guid? cinemaId = null,
        DateOnly? businessDate = null,
        string? hallType = null,
        string? language = null,
        CancellationToken cancellationToken = default);
    Task<ShowtimeDetailResponseDto?> GetByIdAsync(Guid showtimeId, CancellationToken cancellationToken = default);
}

using ABCDMall.Modules.Movies.Domain.Entities;

namespace ABCDMall.Modules.Movies.Application.Services.Showtimes;

public interface IShowtimeRepository
{
    Task<IReadOnlyList<Showtime>> GetShowtimesAsync(
        Guid? movieId,
        Guid? cinemaId,
        DateOnly? businessDate,
        string? hallType,
        string? language,
        CancellationToken cancellationToken = default);
    Task<Showtime?> GetShowtimeByIdAsync(Guid showtimeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ShowtimeSeatInventory>> GetSeatMapByShowtimeIdAsync(
        Guid showtimeId,
        CancellationToken cancellationToken = default);
}

using ABCDMall.Modules.Movies.Application.DTOs.Movies;
using ABCDMall.Modules.Movies.Application.DTOs.Showtimes;

namespace ABCDMall.Modules.Movies.Application.Services.Movies;

public interface IMovieQueryService
{
    Task<MovieHomeResponseDto> GetHomeAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MovieListItemResponseDto>> GetListAsync(
        string? status = null,
        CancellationToken cancellationToken = default);
    Task<MovieDetailResponseDto?> GetByIdAsync(Guid movieId, CancellationToken cancellationToken = default);
    Task<MovieShowtimesResponseDto?> GetShowtimesByMovieIdAsync(
        Guid movieId,
        DateOnly? businessDate = null,
        CancellationToken cancellationToken = default);
}

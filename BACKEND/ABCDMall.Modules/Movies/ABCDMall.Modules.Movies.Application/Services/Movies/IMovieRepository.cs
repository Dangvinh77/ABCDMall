using ABCDMall.Modules.Movies.Domain.Entities;

namespace ABCDMall.Modules.Movies.Application.Services.Movies;

public interface IMovieRepository
{
    Task<IReadOnlyList<Movie>> GetMoviesAsync(CancellationToken cancellationToken = default);
    Task<Movie?> GetMovieByIdAsync(Guid movieId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Showtime>> GetShowtimesByMovieIdAsync(
        Guid movieId,
        DateOnly? businessDate,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Cinema>> GetTopCinemasAsync(CancellationToken cancellationToken = default);
}

using ABCDMall.Modules.Movies.Application.Services.Movies;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Catalog;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Movies.Infrastructure.Repositories.Catalog;

public sealed class MovieRepository : IMovieRepository
{
    private readonly MoviesCatalogDbContext _dbContext;

    public MovieRepository(MoviesCatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Movie>> GetMoviesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Movies
            .AsNoTracking()
            .Include(movie => movie.MovieGenres)
                .ThenInclude(movieGenre => movieGenre.Genre)
            .ToListAsync(cancellationToken);
    }

    public async Task<Movie?> GetMovieByIdAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Movies
            .AsNoTracking()
            .Include(movie => movie.MovieGenres)
                .ThenInclude(movieGenre => movieGenre.Genre)
            .FirstOrDefaultAsync(movie => movie.Id == movieId, cancellationToken);
    }

    public async Task<IReadOnlyList<Showtime>> GetShowtimesByMovieIdAsync(
        Guid movieId,
        DateOnly? businessDate,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Showtimes
            .AsNoTracking()
            .Include(showtime => showtime.Cinema)
            .Include(showtime => showtime.Hall)
            .Where(showtime => showtime.MovieId == movieId);

        if (businessDate.HasValue)
        {
            query = query.Where(showtime => showtime.BusinessDate == businessDate.Value);
        }

        return await query
            .OrderBy(showtime => showtime.BusinessDate)
            .ThenBy(showtime => showtime.StartAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Cinema>> GetTopCinemasAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Cinemas
            .AsNoTracking()
            .Include(cinema => cinema.Showtimes.Where(showtime => showtime.Status == ShowtimeStatus.Open))
            .Where(cinema => cinema.IsActive)
            .ToListAsync(cancellationToken);
    }
}

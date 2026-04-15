using ABCDMall.Modules.Movies.Application.DTOs.Showtimes;

namespace ABCDMall.Modules.Movies.Application.Services.Showtimes;

public sealed class ShowtimeQueryService : IShowtimeQueryService
{
    private readonly IShowtimeRepository _showtimeRepository;

    public ShowtimeQueryService(IShowtimeRepository showtimeRepository)
    {
        _showtimeRepository = showtimeRepository;
    }

    public async Task<IReadOnlyList<ShowtimeListItemDto>> GetListAsync(
        Guid? movieId = null,
        Guid? cinemaId = null,
        DateOnly? businessDate = null,
        string? hallType = null,
        string? language = null,
        CancellationToken cancellationToken = default)
    {
        var showtimes = await _showtimeRepository.GetShowtimesAsync(
            movieId,
            cinemaId,
            businessDate,
            hallType,
            language,
            cancellationToken);

        return showtimes
            .Select(showtime => new ShowtimeListItemDto
            {
                ShowtimeId = showtime.Id,
                MovieId = showtime.MovieId,
                MovieTitle = showtime.Movie?.Title ?? string.Empty,
                CinemaId = showtime.CinemaId,
                CinemaName = showtime.Cinema?.Name ?? string.Empty,
                HallId = showtime.HallId,
                HallName = showtime.Hall?.Name ?? string.Empty,
                HallType = showtime.Hall?.HallType.ToString() ?? string.Empty,
                BusinessDate = showtime.BusinessDate,
                StartAtUtc = showtime.StartAtUtc,
                Language = showtime.Language.ToString(),
                BasePrice = showtime.BasePrice,
                Status = showtime.Status.ToString()
            })
            .ToList();
    }

    public async Task<ShowtimeDetailResponseDto?> GetByIdAsync(Guid showtimeId, CancellationToken cancellationToken = default)
    {
        var showtime = await _showtimeRepository.GetShowtimeByIdAsync(showtimeId, cancellationToken);
        if (showtime is null)
        {
            return null;
        }

        return new ShowtimeDetailResponseDto
        {
            ShowtimeId = showtime.Id,
            MovieId = showtime.MovieId,
            MovieTitle = showtime.Movie?.Title ?? string.Empty,
            MovieSlug = showtime.Movie?.Slug ?? string.Empty,
            MoviePosterUrl = showtime.Movie?.PosterUrl,
            CinemaId = showtime.CinemaId,
            CinemaCode = showtime.Cinema?.Code ?? string.Empty,
            CinemaName = showtime.Cinema?.Name ?? string.Empty,
            HallId = showtime.HallId,
            HallCode = showtime.Hall?.HallCode ?? string.Empty,
            HallName = showtime.Hall?.Name ?? string.Empty,
            HallType = showtime.Hall?.HallType.ToString() ?? string.Empty,
            BusinessDate = showtime.BusinessDate,
            StartAtUtc = showtime.StartAtUtc,
            EndAtUtc = showtime.EndAtUtc,
            Language = showtime.Language.ToString(),
            BasePrice = showtime.BasePrice,
            Status = showtime.Status.ToString()
        };
    }
}

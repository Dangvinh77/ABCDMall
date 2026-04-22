using ABCDMall.Modules.Movies.Application.DTOs.Movies;
using ABCDMall.Modules.Movies.Application.DTOs.Showtimes;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;
using ABCDMall.Modules.Movies.Application.Contracts;
using ABCDMall.Modules.Movies.Application.Services.Showtimes;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace ABCDMall.Modules.Movies.Application.Services.Movies;

public sealed class MovieQueryService : IMovieQueryService
{
    private readonly IMapper _mapper;
    private readonly IMovieRepository _movieRepository;
    private readonly IShowtimeBookingPolicy _showtimeBookingPolicy;
    private readonly ILogger<MovieQueryService> _logger;

    public MovieQueryService(
        IMovieRepository movieRepository,
        IMapper mapper,
        IShowtimeBookingPolicy showtimeBookingPolicy,
        ILogger<MovieQueryService> logger)
    {
        _movieRepository = movieRepository;
        _mapper = mapper;
        _showtimeBookingPolicy = showtimeBookingPolicy;
        _logger = logger;
    }

    public async Task<MovieHomeResponseDto> GetHomeAsync(CancellationToken cancellationToken = default)
    {
        var movies = await _movieRepository.GetMoviesAsync(cancellationToken);
        var topCinemas = await _movieRepository.GetTopCinemasAsync(cancellationToken);

        var featuredMovies = movies
            .Where(IsFeaturedMovie)
            .OrderByDescending(movie => movie.ReleaseDate)
            .ThenBy(movie => movie.Title)
            .Take(5)
            .ToList();

        var nowShowing = movies
            .Where(movie => movie.Status == MovieStatus.NowShowing)
            .OrderBy(movie => movie.Title)
            .Take(10)
            .ToList();

        var comingSoon = movies
            .Where(movie => movie.Status == MovieStatus.ComingSoon)
            .OrderBy(movie => movie.ReleaseDate)
            .ThenBy(movie => movie.Title)
            .Take(10)
            .ToList();

        _logger.LogInformation(
            "Prepared movies home response with {FeaturedCount} featured, {NowShowingCount} now showing, {ComingSoonCount} coming soon movies.",
            featuredMovies.Count,
            nowShowing.Count,
            comingSoon.Count);

        return new MovieHomeResponseDto
        {
            FeaturedMovies = _mapper.Map<IReadOnlyList<MovieListItemResponseDto>>(featuredMovies),
            NowShowing = _mapper.Map<IReadOnlyList<MovieListItemResponseDto>>(nowShowing),
            ComingSoon = _mapper.Map<IReadOnlyList<MovieListItemResponseDto>>(comingSoon),
            TopCinemas = topCinemas
                .Select(cinema => new MovieHomeCinemaDto
                {
                    CinemaId = cinema.Id,
                    Code = cinema.Code,
                    Name = cinema.Name,
                    City = cinema.City,
                    UpcomingShowtimeCount = cinema.Showtimes.Count(showtime => showtime.Status == ShowtimeStatus.Open)
                })
                .OrderByDescending(cinema => cinema.UpcomingShowtimeCount)
                .ThenBy(cinema => cinema.Name)
                .Take(5)
                .ToList()
        };
    }

    public async Task<IReadOnlyList<MovieListItemResponseDto>> GetListAsync(
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var movies = await _movieRepository.GetMoviesAsync(cancellationToken);

        var filteredMovies = movies
            .Where(movie => MatchesStatus(movie, status))
            .OrderBy(movie => movie.Title)
            .ToList();

        _logger.LogInformation(
            "Fetched {MovieCount} movies using status filter {StatusFilter}.",
            filteredMovies.Count,
            string.IsNullOrWhiteSpace(status) ? "all" : status.Trim());

        return _mapper.Map<IReadOnlyList<MovieListItemResponseDto>>(filteredMovies);
    }

    public async Task<MovieDetailResponseDto?> GetByIdAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        var movie = await _movieRepository.GetMovieByIdAsync(movieId, cancellationToken);
        if (movie is null)
        {
            _logger.LogWarning("Movie {MovieId} was not found.", movieId);
            return null;
        }

        _logger.LogInformation("Fetched movie detail for movie {MovieId}.", movieId);

        return _mapper.Map<MovieDetailResponseDto>(movie);
    }

    public async Task<MovieShowtimesResponseDto?> GetShowtimesByMovieIdAsync(
        Guid movieId,
        DateOnly? businessDate = null,
        CancellationToken cancellationToken = default)
    {
        var movie = await _movieRepository.GetMovieByIdAsync(movieId, cancellationToken);
        if (movie is null)
        {
            _logger.LogWarning("Movie showtimes were requested for missing movie {MovieId}.", movieId);
            return null;
        }

        var utcNow = DateTime.UtcNow;
        var showtimes = (await _movieRepository.GetShowtimesByMovieIdAsync(movieId, businessDate, cancellationToken))
            .Where(showtime => _showtimeBookingPolicy.IsVisibleForUser(showtime, utcNow))
            .ToList();
        _logger.LogInformation(
            "Fetched {ShowtimeCount} showtimes for movie {MovieId} with businessDate filter {BusinessDate}.",
            showtimes.Count,
            movieId,
            businessDate?.ToString("yyyy-MM-dd") ?? "all");

        return new MovieShowtimesResponseDto
        {
            MovieId = movie.Id,
            Title = movie.Title,
            Slug = movie.Slug,
            PosterUrl = movie.PosterUrl,
            Dates = showtimes
                .GroupBy(showtime => showtime.BusinessDate)
                .OrderBy(group => group.Key)
                .Select(group => new MovieShowtimeDateGroupDto
                {
                    BusinessDate = group.Key,
                    Cinemas = group
                        .GroupBy(showtime => showtime.CinemaId)
                        .Select(cinemaGroup =>
                        {
                            var firstShowtime = cinemaGroup.First();

                            return new MovieCinemaShowtimesDto
                            {
                                CinemaId = cinemaGroup.Key,
                                CinemaCode = firstShowtime.Cinema?.Code ?? string.Empty,
                                CinemaName = firstShowtime.Cinema?.Name ?? string.Empty,
                                Showtimes = cinemaGroup
                                    .OrderBy(showtime => showtime.StartAtUtc)
                                    .Select(showtime => MapShowtime(showtime, utcNow))
                                    .ToList()
                            };
                        })
                        .OrderBy(cinema => cinema.CinemaName)
                        .ToList()
                })
                .ToList()
        };
    }

    private static bool MatchesStatus(Movie movie, string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return true;
        }

        return string.Equals(movie.Status.ToString(), status.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsFeaturedMovie(Movie movie)
    {
        return movie.Status is MovieStatus.NowShowing or MovieStatus.ComingSoon;
    }

    private ShowtimeResponseDto MapShowtime(Showtime showtime, DateTime utcNow)
    {
        var bookingDecision = _showtimeBookingPolicy.EvaluateForUser(showtime, utcNow);

        return new ShowtimeResponseDto
        {
            ShowtimeId = showtime.Id,
            MovieId = showtime.MovieId,
            CinemaId = showtime.CinemaId,
            HallId = showtime.HallId,
            HallType = showtime.Hall is null ? string.Empty : MoviesContractValueMapper.ToContractValue(showtime.Hall.HallType),
            BusinessDate = showtime.BusinessDate,
            StartAtUtc = showtime.StartAtUtc,
            EndAtUtc = showtime.EndAtUtc,
            Language = MoviesContractValueMapper.ToContractValue(showtime.Language),
            BasePrice = showtime.BasePrice,
            Status = showtime.Status.ToString(),
            IsBookable = bookingDecision.IsBookable,
            BookingUnavailableReason = bookingDecision.UnavailableReason
        };
    }
}

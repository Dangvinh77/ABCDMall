using ABCDMall.Modules.Movies.Application.Contracts;
using ABCDMall.Modules.Movies.Application.DTOs.Showtimes;
using ABCDMall.Modules.Movies.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ABCDMall.Modules.Movies.Application.Services.Showtimes;

public sealed class ShowtimeQueryService : IShowtimeQueryService
{
    private readonly IShowtimeRepository _showtimeRepository;
    private readonly IShowtimeBookingPolicy _showtimeBookingPolicy;
    private readonly ILogger<ShowtimeQueryService> _logger;

    public ShowtimeQueryService(
        IShowtimeRepository showtimeRepository,
        IShowtimeBookingPolicy showtimeBookingPolicy,
        ILogger<ShowtimeQueryService> logger)
    {
        _showtimeRepository = showtimeRepository;
        _showtimeBookingPolicy = showtimeBookingPolicy;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ShowtimeListItemDto>> GetListAsync(
        Guid? movieId = null,
        Guid? cinemaId = null,
        DateOnly? businessDate = null,
        string? hallType = null,
        string? language = null,
        CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        var showtimes = (await _showtimeRepository.GetShowtimesAsync(
            movieId,
            cinemaId,
            businessDate,
            hallType,
            language,
            cancellationToken))
            .Where(showtime => _showtimeBookingPolicy.IsVisibleForUser(showtime, utcNow))
            .ToList();

        _logger.LogInformation(
            "Fetched {ShowtimeCount} showtimes with filters movieId={MovieId}, cinemaId={CinemaId}, businessDate={BusinessDate}, hallType={HallType}, language={Language}.",
            showtimes.Count,
            movieId,
            cinemaId,
            businessDate?.ToString("yyyy-MM-dd") ?? "all",
            hallType ?? "all",
            language ?? "all");

        return showtimes
            .Select(showtime => MapListItem(showtime, utcNow))
            .ToList();
    }

    public async Task<ShowtimeDetailResponseDto?> GetByIdAsync(Guid showtimeId, CancellationToken cancellationToken = default)
    {
        var showtime = await _showtimeRepository.GetShowtimeByIdAsync(showtimeId, cancellationToken);
        if (showtime is null)
        {
            _logger.LogWarning("Showtime {ShowtimeId} was not found.", showtimeId);
            return null;
        }

        _logger.LogInformation("Fetched showtime detail for showtime {ShowtimeId}.", showtimeId);

        var bookingDecision = _showtimeBookingPolicy.EvaluateForUser(showtime, DateTime.UtcNow);

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

    private ShowtimeListItemDto MapListItem(Showtime showtime, DateTime utcNow)
    {
        var bookingDecision = _showtimeBookingPolicy.EvaluateForUser(showtime, utcNow);

        return new ShowtimeListItemDto
        {
            ShowtimeId = showtime.Id,
            MovieId = showtime.MovieId,
            MovieTitle = showtime.Movie?.Title ?? string.Empty,
            CinemaId = showtime.CinemaId,
            CinemaName = showtime.Cinema?.Name ?? string.Empty,
            HallId = showtime.HallId,
            HallName = showtime.Hall?.Name ?? string.Empty,
            HallType = showtime.Hall is null ? string.Empty : MoviesContractValueMapper.ToContractValue(showtime.Hall.HallType),
            BusinessDate = showtime.BusinessDate,
            StartAtUtc = showtime.StartAtUtc,
            Language = MoviesContractValueMapper.ToContractValue(showtime.Language),
            BasePrice = showtime.BasePrice,
            Status = showtime.Status.ToString(),
            IsBookable = bookingDecision.IsBookable,
            BookingUnavailableReason = bookingDecision.UnavailableReason
        };
    }
}

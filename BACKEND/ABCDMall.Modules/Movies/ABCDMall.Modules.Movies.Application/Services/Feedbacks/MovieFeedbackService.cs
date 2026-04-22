using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ABCDMall.Modules.Movies.Application.DTOs.Feedbacks;
using ABCDMall.Modules.Movies.Application.Services.Movies;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;

namespace ABCDMall.Modules.Movies.Application.Services.Feedbacks;

public sealed class MovieFeedbackService : IMovieFeedbackService
{
    private const int MaxPageSize = 50;

    private readonly IMovieRepository _movieRepository;
    private readonly IMovieFeedbackRepository _feedbackRepository;

    public MovieFeedbackService(
        IMovieRepository movieRepository,
        IMovieFeedbackRepository feedbackRepository)
    {
        _movieRepository = movieRepository;
        _feedbackRepository = feedbackRepository;
    }

    public async Task<MovieFeedbackListResponseDto> GetByMovieAsync(
        Guid movieId,
        int? rating,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (rating is < 1 or > 5)
        {
            throw new InvalidOperationException("Rating filter must be between 1 and 5.");
        }

        var normalizedPage = Math.Max(page, 1);
        var normalizedPageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var aggregateFeedbacks = await _feedbackRepository.GetVisibleForAggregateAsync(movieId, cancellationToken);
        var items = await _feedbackRepository.GetVisibleByMovieAsync(
            movieId,
            rating,
            normalizedPage,
            normalizedPageSize,
            cancellationToken);

        var totalCount = rating.HasValue
            ? aggregateFeedbacks.Count(x => x.Rating == rating.Value)
            : aggregateFeedbacks.Count;

        var averageRating = aggregateFeedbacks.Count == 0
            ? 0m
            : decimal.Round((decimal)aggregateFeedbacks.Average(x => x.Rating), 2, MidpointRounding.AwayFromZero);

        return new MovieFeedbackListResponseDto
        {
            MovieId = movieId,
            RatingFilter = rating,
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            TotalCount = totalCount,
            AverageRating = averageRating,
            RatingBreakdown = Enumerable.Range(1, 5)
                .Reverse()
                .ToDictionary(star => star, star => aggregateFeedbacks.Count(x => x.Rating == star)),
            Items = items.Select(Map).ToArray()
        };
    }

    public async Task<MovieFeedbackResponseDto> CreateForMovieAsync(
        Guid movieId,
        CreateMovieFeedbackRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var movie = await _movieRepository.GetMovieByIdAsync(movieId, cancellationToken);
        if (movie is null)
        {
            throw new InvalidOperationException("Movie not found.");
        }

        var feedback = new MovieFeedback
        {
            Id = Guid.NewGuid(),
            MovieId = movieId,
            Rating = request.Rating,
            Comment = request.Comment.Trim(),
            DisplayName = NormalizeDisplayName(request.DisplayName),
            CreatedByEmail = NormalizeEmail(request.Email),
            TagsJson = SerializeTags(request.Tags),
            ModerationStatus = MovieFeedbackModerationStatus.Approved,
            IsVisible = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var created = await _feedbackRepository.AddAsync(feedback, cancellationToken);
        return Map(created);
    }

    public async Task<PublicMovieFeedbackRequestResponseDto> GetPublicRequestAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var request = await GetRequestFromTokenAsync(token, cancellationToken);
        var movie = await _movieRepository.GetMovieByIdAsync(request.MovieId, cancellationToken);
        var now = DateTime.UtcNow;

        return new PublicMovieFeedbackRequestResponseDto
        {
            FeedbackRequestId = request.Id,
            MovieId = request.MovieId,
            ShowtimeId = request.ShowtimeId,
            MovieTitle = movie?.Title ?? string.Empty,
            AvailableAtUtc = request.AvailableAtUtc,
            ExpiresAtUtc = request.ExpiresAtUtc,
            Status = request.Status.ToString(),
            CanSubmit = CanSubmit(request, now),
            Message = GetRequestMessage(request, now)
        };
    }

    public async Task<MovieFeedbackResponseDto> SubmitByTokenAsync(
        string token,
        SubmitMovieFeedbackByTokenRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var feedbackRequest = await GetRequestFromTokenAsync(token, cancellationToken);
        var now = DateTime.UtcNow;
        if (!CanSubmit(feedbackRequest, now))
        {
            throw new InvalidOperationException(GetRequestMessage(feedbackRequest, now) ?? "Feedback link is not available.");
        }

        var feedback = new MovieFeedback
        {
            Id = Guid.NewGuid(),
            FeedbackRequestId = feedbackRequest.Id,
            BookingId = feedbackRequest.BookingId,
            MovieId = feedbackRequest.MovieId,
            ShowtimeId = feedbackRequest.ShowtimeId,
            Rating = request.Rating,
            Comment = request.Comment.Trim(),
            DisplayName = NormalizeDisplayName(request.DisplayName),
            CreatedByEmail = feedbackRequest.PurchaserEmail,
            TagsJson = SerializeTags(request.Tags),
            ModerationStatus = MovieFeedbackModerationStatus.Approved,
            IsVisible = true,
            CreatedAtUtc = now
        };

        var created = await _feedbackRepository.SubmitByRequestAsync(
            feedbackRequest,
            feedback,
            now,
            cancellationToken);

        return Map(created);
    }

    private async Task<MovieFeedbackRequest> GetRequestFromTokenAsync(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("Feedback link is invalid.");
        }

        var tokenHash = HashToken(token.Trim());
        var request = await _feedbackRepository.GetRequestByTokenHashAsync(tokenHash, cancellationToken);
        return request ?? throw new InvalidOperationException("Feedback link is invalid.");
    }

    private static bool CanSubmit(MovieFeedbackRequest request, DateTime utcNow)
    {
        return request.Status == MovieFeedbackRequestStatus.Sent
            && request.SentAtUtc.HasValue
            && request.ExpiresAtUtc.HasValue
            && request.ExpiresAtUtc.Value > utcNow
            && request.SubmittedAtUtc is null
            && request.InvalidatedAtUtc is null;
    }

    private static string? GetRequestMessage(MovieFeedbackRequest request, DateTime utcNow)
    {
        if (request.Status == MovieFeedbackRequestStatus.Submitted || request.SubmittedAtUtc.HasValue)
        {
            return "Feedback link was already used.";
        }

        if (request.ExpiresAtUtc.HasValue && request.ExpiresAtUtc.Value <= utcNow)
        {
            return "Feedback link has expired.";
        }

        if (request.Status == MovieFeedbackRequestStatus.Pending)
        {
            return "Feedback link is not ready yet.";
        }

        if (request.Status is MovieFeedbackRequestStatus.Cancelled or MovieFeedbackRequestStatus.Expired)
        {
            return "Feedback link is no longer valid.";
        }

        return null;
    }

    private static MovieFeedbackResponseDto Map(MovieFeedback feedback)
    {
        return new MovieFeedbackResponseDto
        {
            Id = feedback.Id,
            MovieId = feedback.MovieId,
            ShowtimeId = feedback.ShowtimeId,
            Rating = feedback.Rating,
            Comment = feedback.Comment,
            DisplayName = string.IsNullOrWhiteSpace(feedback.DisplayName) ? "ABCD guest" : feedback.DisplayName,
            Tags = DeserializeTags(feedback.TagsJson),
            ModerationStatus = feedback.ModerationStatus.ToString(),
            CreatedAtUtc = feedback.CreatedAtUtc
        };
    }

    private static string? NormalizeDisplayName(string? displayName)
    {
        return string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();
    }

    private static string? NormalizeEmail(string? email)
    {
        return string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
    }

    private static string? SerializeTags(IReadOnlyCollection<string> tags)
    {
        var normalizedTags = tags
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToArray();

        return normalizedTags.Length == 0 ? null : JsonSerializer.Serialize(normalizedTags);
    }

    private static IReadOnlyCollection<string> DeserializeTags(string? tagsJson)
    {
        if (string.IsNullOrWhiteSpace(tagsJson))
        {
            return Array.Empty<string>();
        }

        return JsonSerializer.Deserialize<IReadOnlyCollection<string>>(tagsJson)
            ?? Array.Empty<string>();
    }

    private static string HashToken(string token)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hashBytes);
    }
}

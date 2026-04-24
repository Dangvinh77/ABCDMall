using System.Security.Cryptography;
using System.Text;
using ABCDMall.Modules.Movies.Application.DTOs.Feedbacks;
using ABCDMall.Modules.Movies.Application.Services.Feedbacks;
using ABCDMall.Modules.Movies.Application.Services.Movies;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;
using Xunit;

namespace ABCDMall.Modules.Movies.Tests;

public sealed class MovieFeedbackServiceTests
{
    [Fact]
    public async Task GetPublicRequestAsync_should_record_first_open_timestamp()
    {
        var request = BuildSentRequest(firstOpenedAtUtc: null, feedbackCount: 0);
        var repository = new FakeMovieFeedbackRepository(request);
        var service = CreateService(repository);

        var result = await service.GetPublicRequestAsync(PlainToken, CancellationToken.None);

        Assert.True(result.CanSubmit);
        Assert.NotNull(result.FirstOpenedAtUtc);
        Assert.Equal(3, result.RemainingSubmissions);
    }

    [Fact]
    public async Task GetPublicRequestAsync_should_expire_opened_request_after_seven_days_without_submission()
    {
        var request = BuildSentRequest(DateTime.UtcNow.AddDays(-8), feedbackCount: 0);
        var repository = new FakeMovieFeedbackRepository(request);
        var service = CreateService(repository);

        var result = await service.GetPublicRequestAsync(PlainToken, CancellationToken.None);

        Assert.False(result.CanSubmit);
        Assert.Equal("Expired", result.Status);
        Assert.Equal("OpenedNoSubmission7Days", result.ExpiredReason);
    }

    [Fact]
    public async Task GetPublicRequestAsync_should_expire_request_after_force_expire_opened_mutation()
    {
        var request = BuildSentRequest(DateTime.UtcNow.AddDays(-1), feedbackCount: 0);
        request.LastOpenedAtUtc = request.FirstOpenedAtUtc;

        var repository = new FakeMovieFeedbackRepository(request);
        request.FirstOpenedAtUtc = DateTime.UtcNow.AddDays(-8);
        request.LastOpenedAtUtc = DateTime.UtcNow.AddDays(-8);
        request.Status = MovieFeedbackRequestStatus.Sent;
        request.ExpiredReason = null;
        request.InvalidatedAtUtc = null;

        var service = CreateService(repository);

        var result = await service.GetPublicRequestAsync(PlainToken, CancellationToken.None);

        Assert.False(result.CanSubmit);
        Assert.Equal("Expired", result.Status);
        Assert.Equal("OpenedNoSubmission7Days", result.ExpiredReason);
        Assert.Equal("Feedback link expired after 7 days without a submission.", result.Message);
    }

    [Fact]
    public async Task SubmitByTokenAsync_should_close_the_link_after_the_third_submission()
    {
        var request = BuildSentRequest(DateTime.UtcNow.AddDays(-1), feedbackCount: 2);
        var repository = new FakeMovieFeedbackRepository(request);
        var service = CreateService(repository);

        await service.SubmitByTokenAsync(PlainToken, new SubmitMovieFeedbackByTokenRequestDto
        {
            Rating = 5,
            Comment = "Great movie"
        }, CancellationToken.None);

        var result = await service.GetPublicRequestAsync(PlainToken, CancellationToken.None);

        Assert.False(result.CanSubmit);
        Assert.Equal("Submitted", result.Status);
        Assert.Equal(0, result.RemainingSubmissions);
        Assert.Equal("SubmissionLimitReached", result.ExpiredReason);
    }

    private static MovieFeedbackService CreateService(FakeMovieFeedbackRepository repository)
    {
        return new MovieFeedbackService(new FakeMovieRepository(), repository);
    }

    private static MovieFeedbackRequest BuildSentRequest(DateTime? firstOpenedAtUtc, int feedbackCount)
    {
        var now = DateTime.UtcNow;
        var request = new MovieFeedbackRequest
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
            BookingId = Guid.Parse("20000000-0000-0000-0000-000000000001"),
            MovieId = FakeMovieRepository.MovieId,
            ShowtimeId = Guid.Parse("30000000-0000-0000-0000-000000000001"),
            PurchaserEmail = "guest@example.com",
            TokenHash = HashToken(PlainToken),
            Status = MovieFeedbackRequestStatus.Sent,
            AvailableAtUtc = now.AddHours(-2),
            SentAtUtc = now.AddHours(-1),
            ExpiresAtUtc = now.AddDays(30),
            FirstOpenedAtUtc = firstOpenedAtUtc,
            LastOpenedAtUtc = firstOpenedAtUtc,
            CreatedAtUtc = now.AddDays(-1),
            UpdatedAtUtc = now.AddDays(-1)
        };

        for (var index = 0; index < feedbackCount; index++)
        {
            request.Feedbacks.Add(new MovieFeedback
            {
                Id = Guid.NewGuid(),
                FeedbackRequestId = request.Id,
                BookingId = request.BookingId,
                MovieId = request.MovieId,
                ShowtimeId = request.ShowtimeId,
                Rating = 5,
                Comment = $"Feedback {index + 1}",
                CreatedAtUtc = now.AddMinutes(-(index + 1)),
                ModerationStatus = MovieFeedbackModerationStatus.Approved,
                IsVisible = true
            });
        }

        return request;
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    private const string PlainToken = "feedback-token";

    private sealed class FakeMovieRepository : IMovieRepository
    {
        public static readonly Guid MovieId = Guid.Parse("40000000-0000-0000-0000-000000000001");

        public Task<IReadOnlyList<Movie>> GetMoviesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Movie>>(Array.Empty<Movie>());

        public Task<Movie?> GetMovieByIdAsync(Guid movieId, CancellationToken cancellationToken = default)
        {
            Movie? movie = movieId == MovieId
                ? new Movie
                {
                    Id = MovieId,
                    Title = "ABCD Test Movie",
                    Slug = "abcd-test-movie"
                }
                : null;

            return Task.FromResult(movie);
        }

        public Task<IReadOnlyList<Showtime>> GetShowtimesByMovieIdAsync(Guid movieId, DateOnly? businessDate, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Showtime>>(Array.Empty<Showtime>());

        public Task<IReadOnlyList<Cinema>> GetTopCinemasAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Cinema>>(Array.Empty<Cinema>());
    }

    private sealed class FakeMovieFeedbackRepository : IMovieFeedbackRepository
    {
        private readonly MovieFeedbackRequest _request;

        public FakeMovieFeedbackRepository(MovieFeedbackRequest request)
        {
            _request = request;
        }

        public Task<IReadOnlyList<MovieFeedback>> GetVisibleByMovieAsync(Guid movieId, int? rating, int page, int pageSize, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<MovieFeedback>>(Array.Empty<MovieFeedback>());

        public Task<IReadOnlyList<MovieFeedback>> GetVisibleForAggregateAsync(Guid movieId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<MovieFeedback>>(Array.Empty<MovieFeedback>());

        public Task<MovieFeedback> AddAsync(MovieFeedback feedback, CancellationToken cancellationToken = default)
            => Task.FromResult(feedback);

        public Task<MovieFeedbackRequest?> GetRequestByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
            => Task.FromResult<MovieFeedbackRequest?>(_request.TokenHash == tokenHash ? _request : null);

        public Task<IReadOnlyList<MovieFeedbackRequest>> GetPendingInvitationRequestsAsync(DateTime utcNow, int take, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<MovieFeedbackRequest>>(Array.Empty<MovieFeedbackRequest>());

        public Task<MovieFeedbackRequest> MarkOpenedAsync(Guid requestId, DateTime utcNow, CancellationToken cancellationToken = default)
        {
            if (!_request.FirstOpenedAtUtc.HasValue)
            {
                _request.FirstOpenedAtUtc = utcNow;
            }

            _request.LastOpenedAtUtc = utcNow;
            _request.UpdatedAtUtc = utcNow;
            return Task.FromResult(_request);
        }

        public Task<MovieFeedbackRequest> MarkInvitationSentAsync(Guid requestId, string tokenHash, DateTime utcNow, CancellationToken cancellationToken = default)
            => Task.FromResult(_request);

        public Task<MovieFeedbackRequest> MarkInvitationFailedAsync(Guid requestId, string error, DateTime utcNow, CancellationToken cancellationToken = default)
            => Task.FromResult(_request);

        public Task<MovieFeedbackRequest> MarkExpiredAsync(Guid requestId, MovieFeedbackRequestExpiredReason reason, DateTime utcNow, CancellationToken cancellationToken = default)
        {
            _request.Status = MovieFeedbackRequestStatus.Expired;
            _request.ExpiredReason = reason;
            _request.InvalidatedAtUtc = utcNow;
            _request.UpdatedAtUtc = utcNow;
            return Task.FromResult(_request);
        }

        public Task<MovieFeedback> SubmitByRequestAsync(MovieFeedbackRequest request, MovieFeedback feedback, DateTime utcNow, CancellationToken cancellationToken = default)
        {
            _request.Feedbacks.Add(feedback);
            _request.UpdatedAtUtc = utcNow;

            if (_request.Feedbacks.Count >= 3)
            {
                _request.Status = MovieFeedbackRequestStatus.Submitted;
                _request.SubmittedAtUtc = utcNow;
                _request.InvalidatedAtUtc = utcNow;
                _request.ExpiredReason = MovieFeedbackRequestExpiredReason.SubmissionLimitReached;
            }

            return Task.FromResult(feedback);
        }
    }
}

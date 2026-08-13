using ABCDMall.Modules.Movies.Application.Services.Feedbacks;
using ABCDMall.Modules.Movies.Application.Services.Showtimes;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;
using ABCDMall.Modules.Movies.Infrastructure.BackgroundServices;
using ABCDMall.Modules.Movies.Infrastructure.Options;
using ABCDMall.Modules.Movies.Infrastructure.Services.Emails;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ABCDMall.Modules.Movies.Tests;

public sealed class MovieFeedbackInvitationBackgroundServiceTests
{
    [Fact]
    public void Constructor_should_allow_service_provider_validation_when_dependencies_are_scoped()
    {
        var request = BuildPendingRequest();
        var repository = new FakeInvitationRepository(request);
        var showtimes = new FakeShowtimeRepository(showtimeEnded: true);
        var sender = new FakeMovieFeedbackEmailSender();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IMovieFeedbackRepository>(_ => repository);
        services.AddScoped<IShowtimeRepository>(_ => showtimes);
        services.AddScoped<IMovieFeedbackEmailSender>(_ => sender);
        services.AddSingleton<IOptions<StripeSettings>>(Options.Create(new StripeSettings
        {
            FrontendBaseUrl = "https://frontend.test"
        }));
        services.AddSingleton<MovieFeedbackInvitationBackgroundService>();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });

        var service = provider.GetRequiredService<MovieFeedbackInvitationBackgroundService>();

        Assert.NotNull(service);
    }

    [Fact]
    public async Task ProcessPendingRequestsOnceAsync_should_send_feedback_email_only_after_showtime_end()
    {
        var request = BuildPendingRequest();
        var repository = new FakeInvitationRepository(request);
        var showtimes = new FakeShowtimeRepository(showtimeEnded: true);
        var sender = new FakeMovieFeedbackEmailSender();
        using var provider = BuildServiceProvider(repository, showtimes, sender);
        var service = new MovieFeedbackInvitationBackgroundService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new StripeSettings { FrontendBaseUrl = "https://frontend.test" }),
            NullLogger<MovieFeedbackInvitationBackgroundService>.Instance);

        await service.ProcessPendingRequestsOnceAsync(CancellationToken.None);

        Assert.Single(sender.Messages);
        Assert.Equal(MovieFeedbackRequestStatus.Sent, request.Status);
        Assert.NotNull(request.SentAtUtc);
        Assert.NotNull(request.TokenHash);
    }

    [Fact]
    public async Task ProcessPendingRequestsOnceAsync_should_not_send_feedback_email_before_showtime_end()
    {
        var request = BuildPendingRequest();
        var repository = new FakeInvitationRepository(request);
        var showtimes = new FakeShowtimeRepository(showtimeEnded: false);
        var sender = new FakeMovieFeedbackEmailSender();
        using var provider = BuildServiceProvider(repository, showtimes, sender);
        var service = new MovieFeedbackInvitationBackgroundService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new StripeSettings { FrontendBaseUrl = "https://frontend.test" }),
            NullLogger<MovieFeedbackInvitationBackgroundService>.Instance);

        await service.ProcessPendingRequestsOnceAsync(CancellationToken.None);

        Assert.Empty(sender.Messages);
        Assert.Equal(MovieFeedbackRequestStatus.Pending, request.Status);
        Assert.Null(request.SentAtUtc);
    }

    [Fact]
    public async Task ProcessPendingRequestsOnceAsync_should_send_after_force_finish_unlocks_request_availability()
    {
        var request = BuildPendingRequest();
        request.AvailableAtUtc = DateTime.UtcNow.AddHours(2);

        var repository = new FakeInvitationRepository(request);
        var showtimes = new FakeShowtimeRepository(showtimeEnded: true);
        var sender = new FakeMovieFeedbackEmailSender();
        using var provider = BuildServiceProvider(repository, showtimes, sender);

        request.AvailableAtUtc = DateTime.UtcNow.AddMinutes(-1);

        var service = new MovieFeedbackInvitationBackgroundService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new StripeSettings { FrontendBaseUrl = "https://frontend.test" }),
            NullLogger<MovieFeedbackInvitationBackgroundService>.Instance);

        await service.ProcessPendingRequestsOnceAsync(CancellationToken.None);

        Assert.Single(sender.Messages);
        Assert.Equal(MovieFeedbackRequestStatus.Sent, request.Status);
    }

    private static ServiceProvider BuildServiceProvider(
        FakeInvitationRepository repository,
        FakeShowtimeRepository showtimes,
        FakeMovieFeedbackEmailSender sender)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IMovieFeedbackRepository>(_ => repository);
        services.AddScoped<IShowtimeRepository>(_ => showtimes);
        services.AddScoped<IMovieFeedbackEmailSender>(_ => sender);

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }

    private static MovieFeedbackRequest BuildPendingRequest()
    {
        var now = DateTime.UtcNow;
        return new MovieFeedbackRequest
        {
            Id = Guid.Parse("51000000-0000-0000-0000-000000000001"),
            BookingId = Guid.Parse("52000000-0000-0000-0000-000000000001"),
            MovieId = FakeShowtimeRepository.MovieId,
            ShowtimeId = FakeShowtimeRepository.ShowtimeId,
            PurchaserEmail = "guest@example.com",
            Status = MovieFeedbackRequestStatus.Pending,
            AvailableAtUtc = now.AddMinutes(-30),
            CreatedAtUtc = now.AddHours(-2),
            UpdatedAtUtc = now.AddHours(-2)
        };
    }

    private sealed class FakeInvitationRepository : IMovieFeedbackRepository
    {
        private readonly MovieFeedbackRequest _request;

        public FakeInvitationRepository(MovieFeedbackRequest request)
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
            => Task.FromResult<MovieFeedbackRequest?>(null);

        public Task<IReadOnlyList<MovieFeedbackRequest>> GetPendingInvitationRequestsAsync(DateTime utcNow, int take, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<MovieFeedbackRequest>>([_request]);

        public Task<MovieFeedbackRequest> MarkOpenedAsync(Guid requestId, DateTime utcNow, CancellationToken cancellationToken = default)
            => Task.FromResult(_request);

        public Task<MovieFeedbackRequest> MarkInvitationSentAsync(Guid requestId, string tokenHash, DateTime utcNow, CancellationToken cancellationToken = default)
        {
            _request.TokenHash = tokenHash;
            _request.Status = MovieFeedbackRequestStatus.Sent;
            _request.SentAtUtc = utcNow;
            _request.LastEmailError = null;
            _request.UpdatedAtUtc = utcNow;
            return Task.FromResult(_request);
        }

        public Task<MovieFeedbackRequest> MarkInvitationFailedAsync(Guid requestId, string error, DateTime utcNow, CancellationToken cancellationToken = default)
        {
            _request.EmailRetryCount += 1;
            _request.LastEmailError = error;
            _request.UpdatedAtUtc = utcNow;
            return Task.FromResult(_request);
        }

        public Task<MovieFeedbackRequest> MarkExpiredAsync(Guid requestId, MovieFeedbackRequestExpiredReason reason, DateTime utcNow, CancellationToken cancellationToken = default)
            => Task.FromResult(_request);

        public Task<MovieFeedback> SubmitByRequestAsync(MovieFeedbackRequest request, MovieFeedback feedback, DateTime utcNow, CancellationToken cancellationToken = default)
            => Task.FromResult(feedback);
    }

    private sealed class FakeShowtimeRepository : IShowtimeRepository
    {
        public static readonly Guid ShowtimeId = Guid.Parse("53000000-0000-0000-0000-000000000001");
        public static readonly Guid MovieId = Guid.Parse("54000000-0000-0000-0000-000000000001");
        private readonly bool _showtimeEnded;

        public FakeShowtimeRepository(bool showtimeEnded)
        {
            _showtimeEnded = showtimeEnded;
        }

        public Task<IReadOnlyList<Showtime>> GetShowtimesAsync(Guid? movieId, Guid? cinemaId, DateOnly? businessDate, string? hallType, string? language, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Showtime>>(Array.Empty<Showtime>());

        public Task<Showtime?> GetShowtimeByIdAsync(Guid showtimeId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            Showtime? showtime = showtimeId == ShowtimeId
                ? new Showtime
                {
                    Id = ShowtimeId,
                    MovieId = MovieId,
                    StartAtUtc = now.AddHours(-3),
                    EndAtUtc = _showtimeEnded ? now.AddMinutes(-30) : now.AddMinutes(30),
                    Status = ShowtimeStatus.Open,
                    Movie = new Movie
                    {
                        Id = MovieId,
                        Title = "ABCD Test Movie",
                        Slug = "abcd-test-movie"
                    }
                }
                : null;

            return Task.FromResult(showtime);
        }

        public Task<IReadOnlyList<ShowtimeSeatInventory>> GetSeatMapByShowtimeIdAsync(Guid showtimeId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ShowtimeSeatInventory>>(Array.Empty<ShowtimeSeatInventory>());

        public Task MarkSeatsBookedAsync(Guid showtimeId, IReadOnlyCollection<Guid> seatInventoryIds, DateTime utcNow, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeMovieFeedbackEmailSender : IMovieFeedbackEmailSender
    {
        public List<MovieFeedbackEmailMessage> Messages { get; } = new();

        public Task SendAsync(MovieFeedbackEmailMessage message, CancellationToken cancellationToken = default)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }
    }
}

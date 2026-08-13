using System.Security.Cryptography;
using System.Text;
using ABCDMall.Modules.Movies.Application.Services.Feedbacks;
using ABCDMall.Modules.Movies.Application.Services.Showtimes;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Infrastructure.Options;
using ABCDMall.Modules.Movies.Infrastructure.Services.Emails;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ABCDMall.Modules.Movies.Infrastructure.BackgroundServices;

public sealed class MovieFeedbackInvitationBackgroundService : BackgroundService
{
    private const int BatchSize = 10;
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly StripeSettings _stripeSettings;
    private readonly ILogger<MovieFeedbackInvitationBackgroundService> _logger;

    public MovieFeedbackInvitationBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<StripeSettings> stripeSettings,
        ILogger<MovieFeedbackInvitationBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _stripeSettings = stripeSettings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ProcessPendingRequestsOnceAsync(stoppingToken);

        using var timer = new PeriodicTimer(PollInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessPendingRequestsOnceAsync(stoppingToken);
        }
    }

    public async Task ProcessPendingRequestsOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var feedbackRepository = scope.ServiceProvider.GetRequiredService<IMovieFeedbackRepository>();
        var showtimeRepository = scope.ServiceProvider.GetRequiredService<IShowtimeRepository>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IMovieFeedbackEmailSender>();

        var now = DateTime.UtcNow;
        var requests = await feedbackRepository.GetPendingInvitationRequestsAsync(now, BatchSize, cancellationToken);

        foreach (var request in requests)
        {
            try
            {
                var showtime = await showtimeRepository.GetShowtimeByIdAsync(request.ShowtimeId, cancellationToken);
                if (showtime is null)
                {
                    continue;
                }

                var showtimeEndUtc = showtime.EndAtUtc ?? showtime.StartAtUtc;
                if (showtimeEndUtc > now)
                {
                    continue;
                }

                var token = GenerateFeedbackToken();
                var tokenHash = HashToken(token);
                var feedbackLink = BuildFeedbackLink(token);
                var movieTitle = showtime.Movie?.Title ?? "your movie";

                await emailSender.SendAsync(new MovieFeedbackEmailMessage
                {
                    ToEmail = request.PurchaserEmail,
                    ToName = "ABCD Cinema guest",
                    Subject = $"Share your feedback for {movieTitle}",
                    HtmlBody = BuildEmailBody(movieTitle, showtimeEndUtc, feedbackLink)
                }, cancellationToken);

                await feedbackRepository.MarkInvitationSentAsync(request.Id, tokenHash, now, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                await feedbackRepository.MarkInvitationFailedAsync(request.Id, ex.Message, now, cancellationToken);
                _logger.LogWarning(ex, "Failed to send movie feedback invitation for request {FeedbackRequestId}.", request.Id);
            }
        }
    }

    private string BuildFeedbackLink(string token)
    {
        var baseUrl = string.IsNullOrWhiteSpace(_stripeSettings.FrontendBaseUrl)
            ? "http://localhost:5173"
            : _stripeSettings.FrontendBaseUrl.TrimEnd('/');

        return $"{baseUrl}/movies/feedback/{Uri.EscapeDataString(token)}";
    }

    private static string BuildEmailBody(string movieTitle, DateTime showtimeEndUtc, string feedbackLink)
    {
        var showtimeText = DateTime.SpecifyKind(showtimeEndUtc, DateTimeKind.Utc).ToLocalTime().ToString("dd/MM/yyyy HH:mm");
        return $$"""
            <p>Hello,</p>
            <p>Your showtime for <strong>{{System.Net.WebUtility.HtmlEncode(movieTitle)}}</strong> has ended.</p>
            <p>
                Share your feedback here:
                <a href="{{System.Net.WebUtility.HtmlEncode(feedbackLink)}}">Open movie feedback</a>.
            </p>
            <p>
                This link allows up to 3 feedback submissions.
                If you open the link but do not submit any feedback, it expires 7 days after the first open.
            </p>
            <p><strong>Showtime ended:</strong> {{System.Net.WebUtility.HtmlEncode(showtimeText)}}</p>
            <p>ABCD Cinema</p>
            """;
    }

    private static string GenerateFeedbackToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string HashToken(string token)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hashBytes);
    }
}

using ABCDMall.Modules.Movies.Application.Services.Bookings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ABCDMall.Modules.Movies.Infrastructure.BackgroundServices;

public sealed class BookingHoldCleanupBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingHoldCleanupBackgroundService> _logger;

    public BookingHoldCleanupBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<BookingHoldCleanupBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        while (!stoppingToken.IsCancellationRequested
            && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IBookingHoldRepository>();

                var expiredCount = await repository.ExpireAsync(DateTime.UtcNow, stoppingToken);
                if (expiredCount > 0)
                {
                    _logger.LogInformation("Expired {ExpiredCount} booking holds.", expiredCount);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cleanup expired booking holds.");
            }
        }
    }
}

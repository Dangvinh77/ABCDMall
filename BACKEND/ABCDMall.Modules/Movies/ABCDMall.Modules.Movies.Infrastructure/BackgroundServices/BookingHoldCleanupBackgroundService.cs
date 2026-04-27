using ABCDMall.Modules.Movies.Application.Services.Bookings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ABCDMall.Modules.Movies.Infrastructure.BackgroundServices
{
    public sealed class BookingHoldCleanupBackgroundService : BackgroundService
    {
        private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(1);

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
            // BackgroundService is a singleton, while repository/DbContext are scoped.
            // Create a scope for each cleanup run so EF Core uses the correct lifetime.
            _logger.LogInformation("Booking hold cleanup background service started.");

            try
            {
                // Run once at startup to expire holds left active before the app restarted.
                await CleanupExpiredHoldsAsync(stoppingToken);

                using var timer = new PeriodicTimer(CleanupInterval);

                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await CleanupExpiredHoldsAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Normal shutdown path.
                _logger.LogInformation("Booking hold cleanup background service is stopping.");
            }
        }

        private async Task CleanupExpiredHoldsAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IBookingHoldRepository>();

                var expiredCount = await repository.ExpireAsync(DateTime.UtcNow, cancellationToken);
                if (expiredCount > 0)
                {
                    _logger.LogInformation("Expired {ExpiredCount} booking holds.", expiredCount);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Log the error but don't crash - database may not be ready yet
                _logger.LogWarning(ex, "Failed to cleanup expired booking holds.");
            }
        }
    }
}

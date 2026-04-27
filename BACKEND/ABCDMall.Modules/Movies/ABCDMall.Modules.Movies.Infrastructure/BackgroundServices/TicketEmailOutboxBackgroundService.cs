using System.Text.Json;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking;
using ABCDMall.Modules.Movies.Infrastructure.Services.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ABCDMall.Modules.Movies.Infrastructure.BackgroundServices;

public sealed class TicketEmailOutboxBackgroundService : BackgroundService
{
    private const int MaxRetryCount = 3;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TicketEmailOutboxBackgroundService> _logger;

    public TicketEmailOutboxBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<TicketEmailOutboxBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Ticket email outbox background service started.");

        try
        {
            await ProcessPendingEventsAsync(stoppingToken);

            using var timer = new PeriodicTimer(PollInterval);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ProcessPendingEventsAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Ticket email outbox background service is stopping.");
        }
    }

    private async Task ProcessPendingEventsAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MoviesBookingDbContext>();
            var dispatcher = scope.ServiceProvider.GetRequiredService<ITicketEmailDispatcher>();

            var events = await dbContext.OutboxEvents
                .Where(x => x.EventType == TicketEmailOutboxEvent.EventType
                    && (x.Status == "Pending" || (x.Status == "Failed" && x.RetryCount < MaxRetryCount)))
                .OrderBy(x => x.OccurredAtUtc)
                .Take(10)
                .ToListAsync(cancellationToken);

            foreach (var outboxEvent in events)
            {
                Guid? bookingId = null;
                try
                {
                    bookingId = ReadBookingId(outboxEvent.PayloadJson);
                    await dispatcher.SendTicketEmailAsync(bookingId.Value, cancellationToken);

                    outboxEvent.Status = "Processed";
                    outboxEvent.ProcessedAtUtc = DateTime.UtcNow;
                    outboxEvent.LastError = null;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    outboxEvent.Status = "Failed";
                    outboxEvent.RetryCount++;
                    outboxEvent.LastError = ex.Message;
                    outboxEvent.ProcessedAtUtc = null;

                    if (bookingId.HasValue)
                    {
                        await MarkTicketDeliveryFailedAsync(dbContext, bookingId.Value, ex.Message, cancellationToken);
                    }

                    _logger.LogWarning(ex, "Failed to send ticket email for outbox event {OutboxEventId}.", outboxEvent.Id);
                }
            }

            if (events.Count > 0)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            // Log the error but don't crash - database may not be ready yet
            _logger.LogWarning(ex, "Failed to process ticket email outbox events.");
        }
    }

    private static Guid ReadBookingId(string payloadJson)
    {
        using var document = JsonDocument.Parse(payloadJson);
        var value = document.RootElement.GetProperty("bookingId").GetString();
        return Guid.TryParse(value, out var bookingId)
            ? bookingId
            : throw new InvalidOperationException("Ticket email outbox payload has an invalid bookingId.");
    }

    private static async Task MarkTicketDeliveryFailedAsync(
        MoviesBookingDbContext dbContext,
        Guid bookingId,
        string error,
        CancellationToken cancellationToken)
    {
        var tickets = await dbContext.Tickets
            .Where(x => x.BookingId == bookingId)
            .ToListAsync(cancellationToken);

        foreach (var ticket in tickets)
        {
            ticket.DeliveryStatus = TicketDeliveryStatuses.EmailFailed;
            ticket.EmailSendError = error;
            ticket.UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}

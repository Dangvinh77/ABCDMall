using ABCDMall.Modules.Events.Application.Services.Events;
using ABCDMall.Modules.Events.Domain.Entities;
using ABCDMall.Modules.Events.Domain.Enums;
using ABCDMall.Modules.Events.Infrastructure.Persistence.Events;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Events.Infrastructure.Repositories;

public class EventRepository : IEventRepository
{
    private readonly EventsDbContext _dbContext;

    public EventRepository(EventsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Event>> GetEventsAsync(bool includeRejected, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Events
            .AsNoTracking()
            .AsQueryable();

        if (!includeRejected)
        {
            query = query.Where(x => x.ApprovalStatus == EventApprovalStatus.Approved);
        }

        return await query
            .OrderBy(x => x.StartDateTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetEventsByShopIdAsync(string shopId, bool includeRejected, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Events
            .AsNoTracking()
            .Where(x => x.ShopId == shopId);

        if (!includeRejected)
        {
            query = query.Where(x => x.ApprovalStatus == EventApprovalStatus.Approved);
        }

        return await query.OrderBy(x => x.StartDateTime).ToListAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<Event>> GetFloorConflictsAsync(
        EventLocationType locationType,
        DateTime startDateTime,
        DateTime endDateTime,
        Guid? excludeEventId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Events
            .AsNoTracking()
            .Where(x =>
                x.LocationType == locationType
                && (x.ApprovalStatus == EventApprovalStatus.Pending || x.ApprovalStatus == EventApprovalStatus.Approved)
                && x.StartDateTime < endDateTime
                && x.EndDateTime > startDateTime);

        if (excludeEventId.HasValue)
        {
            query = query.Where(x => x.Id != excludeEventId.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task CreateRegistrationAsync(EventRegistration registration, CancellationToken cancellationToken = default)
    {
        await _dbContext.EventRegistrations.AddAsync(registration, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Event?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task CreateEventAsync(Event ev, CancellationToken cancellationToken = default)
    {
        await _dbContext.Events.AddAsync(ev, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateEventAsync(Event ev, CancellationToken cancellationToken = default)
    {
        _dbContext.Events.Update(ev);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteEventAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Events
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null) return;

        _dbContext.Events.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
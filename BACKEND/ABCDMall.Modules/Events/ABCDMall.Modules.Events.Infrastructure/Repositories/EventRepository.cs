using ABCDMall.Modules.Events.Application.Services.Events;
using ABCDMall.Modules.Events.Domain.Entities;
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

    public async Task<IReadOnlyList<Event>> GetEventsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Events
            .AsNoTracking()
            .OrderByDescending(x => x.IsHot)
            .ThenByDescending(x => x.StartDate)
            .ToListAsync(cancellationToken);
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
using ABCDMall.Modules.Events.Domain.Entities;

namespace ABCDMall.Modules.Events.Application.Services.Events;

public interface IEventRepository
{
    Task<IReadOnlyList<Event>> GetEventsAsync(CancellationToken cancellationToken = default);
    Task<Event?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateEventAsync(Event ev, CancellationToken cancellationToken = default);
    Task UpdateEventAsync(Event ev, CancellationToken cancellationToken = default);
    Task DeleteEventAsync(Guid id, CancellationToken cancellationToken = default);
}
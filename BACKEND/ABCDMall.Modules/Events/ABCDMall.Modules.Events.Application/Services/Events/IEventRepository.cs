using ABCDMall.Modules.Events.Domain.Entities;
using ABCDMall.Modules.Events.Domain.Enums;

namespace ABCDMall.Modules.Events.Application.Services.Events;

public interface IEventRepository
{
    Task<IReadOnlyList<Event>> GetEventsAsync(bool includeRejected, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetEventsByShopIdAsync(string shopId, bool includeRejected, CancellationToken cancellationToken = default);
    Task<Event?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetFloorConflictsAsync(
        EventLocationType locationType,
        DateTime startDateTime,
        DateTime endDateTime,
        Guid? excludeEventId,
        CancellationToken cancellationToken = default);
    Task CreateEventAsync(Event ev, CancellationToken cancellationToken = default);
    Task UpdateEventAsync(Event ev, CancellationToken cancellationToken = default);
    Task DeleteEventAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateRegistrationAsync(EventRegistration registration, CancellationToken cancellationToken = default);
}
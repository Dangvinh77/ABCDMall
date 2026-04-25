using ABCDMall.Modules.Events.Application.DTOs;
using ABCDMall.Modules.Events.Application.DTOs.Events;

namespace ABCDMall.Modules.Events.Application.Services.Events;

public interface IEventQueryService
{
    Task<IReadOnlyList<EventDto>> GetListAsync(EventListQueryDto query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventDto>> GetManagerEventsAsync(string shopId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventDto>> GetManagerScheduleAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventDto>> GetAdminReviewListAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventDto>> GetPublicEventsAsync(string? filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventDto>> GetPublicShopEventsAsync(string shopId, CancellationToken cancellationToken = default);
    Task<EventDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventDto>> GetActiveEventsAsync(CancellationToken cancellationToken = default);
}
using ABCDMall.Modules.Events.Application.DTOs;
using ABCDMall.Modules.Events.Application.DTOs.Events;

namespace ABCDMall.Modules.Events.Application.Services.Events;

public interface IEventQueryService
{
    Task<IReadOnlyList<EventDto>> GetListAsync(EventListQueryDto query, CancellationToken cancellationToken = default);
    Task<EventDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Lấy danh sách sự kiện HOT cho Banner Slider trang chủ.</summary>
    Task<IReadOnlyList<EventDto>> GetHotEventsAsync(CancellationToken cancellationToken = default);
}
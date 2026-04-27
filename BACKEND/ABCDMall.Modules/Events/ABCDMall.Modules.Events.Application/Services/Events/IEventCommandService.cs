using ABCDMall.Modules.Events.Application.DTOs.Events;

using ABCDMall.Modules.Events.Application.Common;
using ABCDMall.Modules.Events.Application.DTOs.Events;

namespace ABCDMall.Modules.Events.Application.Services.Events;

public interface IEventCommandService
{
    Task<ApplicationResult<Guid>> CreateMallEventAsync(CreateEventRequestDto request, CancellationToken cancellationToken = default);
    Task<ApplicationResult<Guid>> CreateShopEventAsync(string managerShopId, CreateEventRequestDto request, CancellationToken cancellationToken = default);
    Task<ApplicationResult<bool>> UpdateAsync(Guid id, UpdateEventRequestDto request, CancellationToken cancellationToken = default);
    Task<ApplicationResult<bool>> ApproveAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApplicationResult<bool>> RejectAsync(Guid id, string reason, CancellationToken cancellationToken = default);
    Task<ApplicationResult<EventRegistrationResultDto>> RegisterAsync(Guid eventId, RegisterEventRequestDto request, CancellationToken cancellationToken = default);
}
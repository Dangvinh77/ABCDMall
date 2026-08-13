using ABCDMall.Modules.Events.Application.DTOs.Events;

namespace ABCDMall.Modules.Events.Application.Services.Events;

public interface IEventCommandService
{
    Task<Guid> CreateAsync(CreateEventRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Guid id, UpdateEventRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
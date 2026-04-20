using ABCDMall.Modules.Events.Application.DTOs.Events;
using ABCDMall.Modules.Events.Domain.Entities;
using ABCDMall.Modules.Events.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ABCDMall.Modules.Events.Application.Services.Events;

public sealed class EventCommandService : IEventCommandService
{
    private readonly IEventRepository _eventRepository;
    private readonly ILogger<EventCommandService> _logger;

    public EventCommandService(
        IEventRepository eventRepository,
        ILogger<EventCommandService> logger)
    {
        _eventRepository = eventRepository;
        _logger = logger;
    }

    public async Task<Guid> CreateAsync(CreateEventRequestDto request, CancellationToken cancellationToken = default)
    {
        var isBrandEvent = request.EventType == (int)EventType.BrandEvent;
        var normalizedShopId = string.IsNullOrWhiteSpace(request.ShopId) ? null : request.ShopId.Trim();
        var normalizedShopName = string.IsNullOrWhiteSpace(request.ShopName) ? null : request.ShopName.Trim();

        if (isBrandEvent && normalizedShopId is null)
        {
            throw new InvalidOperationException("ShopId is required when EventType is BrandEvent.");
        }

        var ev = new Event
        {
            Id           = Guid.NewGuid(),
            Title        = request.Title.Trim(),
            Description  = request.Description?.Trim() ?? string.Empty,
            CoverImageUrl = request.CoverImageUrl?.Trim() ?? string.Empty,
            StartDate    = request.StartDate,
            EndDate      = request.EndDate,
            Location     = request.Location.Trim(),
            EventType    = (EventType)request.EventType,
            ShopId       = isBrandEvent ? normalizedShopId : null,
            ShopName     = isBrandEvent ? normalizedShopName : null,
            IsHot        = request.IsHot,
            CreatedAt    = DateTime.UtcNow
        };

        await _eventRepository.CreateEventAsync(ev, cancellationToken);

        _logger.LogInformation(
            "Created event {EventId} '{EventTitle}' (type={EventType}, isHot={IsHot}).",
            ev.Id, ev.Title, ev.EventType, ev.IsHot);

        return ev.Id;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateEventRequestDto request, CancellationToken cancellationToken = default)
    {
        var existing = await _eventRepository.GetEventByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            _logger.LogWarning("Cannot update event {EventId} because it does not exist.", id);
            return false;
        }

        var isBrandEvent = request.EventType == (int)EventType.BrandEvent;
        var normalizedShopId = string.IsNullOrWhiteSpace(request.ShopId) ? null : request.ShopId.Trim();
        var normalizedShopName = string.IsNullOrWhiteSpace(request.ShopName) ? null : request.ShopName.Trim();

        if (isBrandEvent && normalizedShopId is null)
        {
            throw new InvalidOperationException("ShopId is required when EventType is BrandEvent.");
        }

        existing.Title        = request.Title.Trim();
        existing.Description  = request.Description?.Trim() ?? string.Empty;
        existing.CoverImageUrl = request.CoverImageUrl?.Trim() ?? string.Empty;
        existing.StartDate    = request.StartDate;
        existing.EndDate      = request.EndDate;
        existing.Location     = request.Location.Trim();
        existing.EventType    = (EventType)request.EventType;
        existing.ShopId       = isBrandEvent ? normalizedShopId : null;
        existing.ShopName     = isBrandEvent ? normalizedShopName : null;
        existing.IsHot        = request.IsHot;

        await _eventRepository.UpdateEventAsync(existing, cancellationToken);

        _logger.LogInformation("Updated event {EventId} '{EventTitle}'.", id, existing.Title);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _eventRepository.GetEventByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            _logger.LogWarning("Cannot delete event {EventId} because it does not exist.", id);
            return false;
        }

        await _eventRepository.DeleteEventAsync(id, cancellationToken);
        _logger.LogInformation("Deleted event {EventId}.", id);
        return true;
    }
}

using ABCDMall.Modules.Events.Application.DTOs;
using ABCDMall.Modules.Events.Application.DTOs.Events;
using ABCDMall.Modules.Events.Domain.Enums;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace ABCDMall.Modules.Events.Application.Services.Events;

public sealed class EventQueryService : IEventQueryService
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<EventQueryService> _logger;

    public EventQueryService(
        IEventRepository eventRepository,
        IMapper mapper,
        ILogger<EventQueryService> logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<EventDto>> GetListAsync(
        EventListQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var events = await _eventRepository.GetEventsAsync(cancellationToken);

        // Filter keyword
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim();
            events = events
                .Where(e => e.Title.Contains(kw, StringComparison.OrdinalIgnoreCase)
                         || e.Description.Contains(kw, StringComparison.OrdinalIgnoreCase)
                         || e.Location.Contains(kw, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // Filter EventType
        if (query.EventType.HasValue && Enum.IsDefined(typeof(EventType), query.EventType.Value))
        {
            var eventType = (EventType)query.EventType.Value;
            events = events.Where(e => e.EventType == eventType).ToList();
        }

        // Filter IsHot
        if (query.IsHot.HasValue)
        {
            events = events.Where(e => e.IsHot == query.IsHot.Value).ToList();
        }

        // Map trước rồi filter Status (Status là computed field trên DTO)
        var dtos = _mapper.Map<IReadOnlyList<EventDto>>(events);

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim().ToLower();
            dtos = dtos.Where(e => e.Status.ToLower() == status).ToList();
        }

        _logger.LogInformation(
            "Fetched {EventCount} events with filters keyword={Keyword}, type={Type}, status={Status}, isHot={IsHot}.",
            dtos.Count, query.Keyword, query.EventType, query.Status, query.IsHot);

        return dtos;
    }

    public async Task<EventDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ev = await _eventRepository.GetEventByIdAsync(id, cancellationToken);
        if (ev is null)
        {
            _logger.LogWarning("Event {EventId} was not found.", id);
            return null;
        }

        return _mapper.Map<EventDto>(ev);
    }

    public async Task<IReadOnlyList<EventDto>> GetHotEventsAsync(CancellationToken cancellationToken = default)
    {
        var query = new EventListQueryDto { IsHot = true };
        return await GetListAsync(query, cancellationToken);
    }
}
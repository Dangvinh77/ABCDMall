using ABCDMall.Modules.Events.Application.DTOs;
using ABCDMall.Modules.Events.Application.DTOs.Events;
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

    public async Task<IReadOnlyList<EventDto>> GetListAsync(EventListQueryDto query, CancellationToken cancellationToken = default)
    {
        var events = string.IsNullOrWhiteSpace(query.ShopId)
            ? await _eventRepository.GetEventsAsync(query.IncludeAllStatuses, cancellationToken)
            : await _eventRepository.GetEventsByShopIdAsync(query.ShopId, query.IncludeAllStatuses, cancellationToken);

        if (query.ApprovalStatus.HasValue)
        {
            events = events.Where(x => (int)x.ApprovalStatus == query.ApprovalStatus.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.Trim();
            events = events.Where(x => x.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) || x.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        events = ApplyTimeFilter(events, query.TimeFilter);
        return _mapper.Map<IReadOnlyList<EventDto>>(events.OrderBy(x => x.StartDateTime).ToList());
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

    public Task<IReadOnlyList<EventDto>> GetManagerEventsAsync(string shopId, CancellationToken cancellationToken = default)
        => GetListAsync(new EventListQueryDto { ShopId = shopId, IncludeAllStatuses = true }, cancellationToken);

    public Task<IReadOnlyList<EventDto>> GetManagerScheduleAsync(CancellationToken cancellationToken = default)
        => GetListAsync(new EventListQueryDto { IncludeAllStatuses = false }, cancellationToken);

    public Task<IReadOnlyList<EventDto>> GetAdminReviewListAsync(CancellationToken cancellationToken = default)
        => GetListAsync(new EventListQueryDto { IncludeAllStatuses = true }, cancellationToken);

    public async Task<IReadOnlyList<EventDto>> GetEventsByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        var events = await _eventRepository.GetEventsAsync(includeRejected: true, cancellationToken);
        
        if (string.IsNullOrWhiteSpace(status))
            return _mapper.Map<IReadOnlyList<EventDto>>(events);

        var filteredEvents = status.Trim().ToLowerInvariant() switch
        {
            "pending" => events.Where(x => (int)x.ApprovalStatus == 1).ToList(),
            "approved" => events.Where(x => (int)x.ApprovalStatus == 2).ToList(),
            "rejected" => events.Where(x => (int)x.ApprovalStatus == 3).ToList(),
            _ => events
        };

        return _mapper.Map<IReadOnlyList<EventDto>>(filteredEvents);
    }

    public Task<IReadOnlyList<EventDto>> GetPublicEventsAsync(string? filter, CancellationToken cancellationToken = default)
        => GetListAsync(new EventListQueryDto { IncludeAllStatuses = false, TimeFilter = filter }, cancellationToken);

    public Task<IReadOnlyList<EventDto>> GetPublicShopEventsAsync(string shopId, CancellationToken cancellationToken = default)
        => GetListAsync(new EventListQueryDto { IncludeAllStatuses = false, ShopId = shopId }, cancellationToken);

    public Task<IReadOnlyList<EventDto>> GetActiveEventsAsync(CancellationToken cancellationToken = default)
        => GetListAsync(new EventListQueryDto { IncludeAllStatuses = false, TimeFilter = "ongoing" }, cancellationToken);

    private static IReadOnlyList<Domain.Entities.Event> ApplyTimeFilter(IReadOnlyList<Domain.Entities.Event> events, string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return events;
        }

        var now = DateTime.UtcNow;
        return filter.Trim().ToLowerInvariant() switch
        {
            "ongoing" => events.Where(x => x.StartDateTime <= now && x.EndDateTime >= now).ToList(),
            "upcoming" => events.Where(x => x.StartDateTime > now).ToList(),
            _ => events
        };
    }
}
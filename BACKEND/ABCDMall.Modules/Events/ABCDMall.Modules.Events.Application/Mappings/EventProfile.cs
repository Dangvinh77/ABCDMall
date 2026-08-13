using ABCDMall.Modules.Events.Application.DTOs;
using ABCDMall.Modules.Events.Domain.Entities;
using ABCDMall.Modules.Events.Domain.Enums;

namespace ABCDMall.Modules.Events.Application.Mappings;

public sealed class EventProfile : AutoMapper.Profile
{
    public EventProfile()
    {
        CreateMap<Event, EventDto>()
            .ForMember(dest => dest.EventType,
                opt => opt.MapFrom(src => src.EventType.ToString()))
            .ForMember(dest => dest.EventTypeId,
                opt => opt.MapFrom(src => (int)src.EventType))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => ComputeStatus(src).ToString()))
            .ForMember(dest => dest.StatusId,
                opt => opt.MapFrom(src => (int)ComputeStatus(src)));
    }

    private static EventStatus ComputeStatus(Event e)
    {
        var now = DateTime.UtcNow;
        if (now < e.StartDate) return EventStatus.Upcoming;
        if (now > e.EndDate)   return EventStatus.Ended;
        return EventStatus.Ongoing;
    }
}
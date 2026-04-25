using ABCDMall.Modules.Events.Application.DTOs;
using ABCDMall.Modules.Events.Domain.Entities;

namespace ABCDMall.Modules.Events.Application.Mappings;

public sealed class EventProfile : AutoMapper.Profile
{
    public EventProfile()
    {
        CreateMap<Event, EventDto>()
            .ForMember(dest => dest.LocationType, opt => opt.MapFrom(src => src.LocationType.ToString()))
            .ForMember(dest => dest.ApprovalStatus, opt => opt.MapFrom(src => src.ApprovalStatus.ToString()))
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.ShopId) ? "Mall" : src.ShopId!))
            .ForMember(dest => dest.IsOngoing, opt => opt.MapFrom(src => src.StartDateTime <= DateTime.UtcNow && src.EndDateTime >= DateTime.UtcNow))
            .ForMember(dest => dest.IsUpcoming, opt => opt.MapFrom(src => src.StartDateTime > DateTime.UtcNow));
    }
}
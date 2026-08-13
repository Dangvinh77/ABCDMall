using ABCDMall.Modules.UtilityMap.Application.DTOs.Maps;
using ABCDMall.Modules.UtilityMap.Domain.Entities;
using AutoMapper;

namespace ABCDMall.Modules.UtilityMap.Application.Mappings;

public class MapProfile : Profile
{
    public MapProfile()
    {
        CreateMap<FloorPlan, FloorPlanDto>();
        CreateMap<MapLocation, MapLocationDto>()
            .ForMember(dest => dest.ShopName, opt => opt.MapFrom(src => 
                (src.Status == "Available" && string.IsNullOrWhiteSpace(src.ShopName)) 
                ? "Available for Rent" 
                : src.ShopName));
    }
}

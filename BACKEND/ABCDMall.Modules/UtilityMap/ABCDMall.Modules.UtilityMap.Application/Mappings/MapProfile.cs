using ABCDMall.Modules.UtilityMap.Application.DTOs.Maps;
using ABCDMall.Modules.UtilityMap.Domain.Entities;
using AutoMapper;

namespace ABCDMall.Modules.UtilityMap.Application.Mappings;

public class MapProfile : Profile
{
    public MapProfile()
    {
        CreateMap<FloorPlan, FloorPlanDto>();
        CreateMap<MapLocation, MapLocationDto>();
    }
}

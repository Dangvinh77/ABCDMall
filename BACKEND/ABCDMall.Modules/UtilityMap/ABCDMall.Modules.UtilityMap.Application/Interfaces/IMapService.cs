using ABCDMall.Modules.UtilityMap.Domain.Entities;
using ABCDMall.Shared.DTOs;

namespace ABCDMall.Modules.UtilityMap.Domain.Interfaces;

public interface IMapService
{
    Task<List<FloorPlanDto>> GetAllFloorsAsync();
    Task<FloorPlanDto?> GetFloorPlanAsync(string floorLevel);
    Task CreateFloorPlanAsync(FloorPlanDto dto);
    Task<bool> AddLocationAsync(int floorPlanId, MapLocationDto dto);
    Task<bool> DeleteLocationAsync(int locationId);
    Task<bool> UpdateFloorPlanAsync(int id, FloorPlanDto dto);

    
}
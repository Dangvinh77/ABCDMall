using ABCDMall.Modules.UtilityMap.Domain.Entities;

namespace ABCDMall.Modules.UtilityMap.Domain.Interfaces;

public interface IMapRepository
{
    Task<List<FloorPlan>> GetAllFloorsAsync();
    Task<FloorPlan?> GetFloorPlanAsync(string floorLevel);  
    Task CreateFloorPlanAsync(FloorPlan floorPlan);
    Task<bool> AddLocationAsync(int floorPlanId, MapLocation location);
    Task<bool> DeleteLocationAsync(int locationId);
    Task<bool> UpdateFloorPlanAsync(int id, string blueprintImageUrl, string description);
}
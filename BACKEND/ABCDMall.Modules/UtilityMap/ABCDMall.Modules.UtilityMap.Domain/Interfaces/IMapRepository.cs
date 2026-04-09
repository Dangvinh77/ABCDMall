using ABCDMall.Modules.UtilityMap.Domain.Entities;

namespace ABCDMall.Modules.UtilityMap.Domain.Interfaces;

public interface IMapRepository
{
    Task<FloorPlan?> GetFloorPlanAsync(string floorLevel);
    Task<List<FloorPlan>> GetAllFloorsAsync();
    Task CreateFloorPlanAsync(FloorPlan floorPlan);
}
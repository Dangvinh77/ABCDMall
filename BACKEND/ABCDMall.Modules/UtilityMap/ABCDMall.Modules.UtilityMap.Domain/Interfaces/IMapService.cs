using ABCDMall.Modules.UtilityMap.Domain.Entities;

namespace ABCDMall.Modules.UtilityMap.Domain.Interfaces;

public interface IMapService
{
    Task<FloorPlan?> GetFloorPlanAsync(string floorLevel);
    Task<List<FloorPlan>> GetAllFloorsAsync();
}
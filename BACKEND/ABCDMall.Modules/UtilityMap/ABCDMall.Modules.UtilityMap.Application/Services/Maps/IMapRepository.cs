using ABCDMall.Modules.UtilityMap.Domain.Entities;

namespace ABCDMall.Modules.UtilityMap.Application.Services.Maps;

public interface IMapRepository
{
    Task<List<FloorPlan>> GetAllFloorsAsync(CancellationToken cancellationToken = default);
    Task<FloorPlan?> GetFloorPlanAsync(string floorLevel, CancellationToken cancellationToken = default);
    Task CreateFloorPlanAsync(FloorPlan floorPlan, CancellationToken cancellationToken = default);
    Task<bool> AddLocationAsync(int floorPlanId, MapLocation location, CancellationToken cancellationToken = default);
    Task<bool> DeleteLocationAsync(int locationId, CancellationToken cancellationToken = default);
    Task<bool> UpdateFloorPlanAsync(int id, FloorPlan floorPlan, CancellationToken cancellationToken = default);
    Task<FloorPlan?> GetFloorPlanByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<MapLocation?> GetLocationByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<MapLocation?> GetLocationByShopInfoIdAsync(string shopInfoId, CancellationToken cancellationToken = default);
    Task UpdateLocationSlotAsync(MapLocation location, CancellationToken cancellationToken = default);
    Task<bool> UpdateLocationStatusByShopInfoIdAsync(string shopInfoId, string status, CancellationToken cancellationToken = default);
    Task<bool> UpdateLocationDetailsByShopInfoIdAsync(string shopInfoId, string shopName, string shopUrl, CancellationToken cancellationToken = default);
}

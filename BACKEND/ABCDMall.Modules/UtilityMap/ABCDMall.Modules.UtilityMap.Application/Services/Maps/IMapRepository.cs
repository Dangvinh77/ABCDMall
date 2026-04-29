using ABCDMall.Modules.UtilityMap.Domain.Entities;

namespace ABCDMall.Modules.UtilityMap.Application.Services.Maps;

public interface IMapRepository
{
    Task<List<FloorPlan>> GetAllFloorsAsync(CancellationToken cancellationToken = default);
    Task<FloorPlan?> GetFloorPlanAsync(string floorLevel, CancellationToken cancellationToken = default);
    Task<FloorPlan?> GetFloorPlanByIdAsync(int id, CancellationToken cancellationToken = default);


    Task CreateFloorPlanAsync(FloorPlan floorPlan, CancellationToken cancellationToken = default);
    Task<bool> UpdateFloorPlanAsync(int id, FloorPlan floorPlan, CancellationToken cancellationToken = default);
    Task<bool> AddLocationAsync(int floorPlanId, MapLocation location, CancellationToken cancellationToken = default);
    Task<bool> DeleteLocationAsync(int locationId, CancellationToken cancellationToken = default);



    /// <summary>Lấy một MapLocation theo ID (không kèm FloorPlan).</summary>
    Task<MapLocation?> GetLocationByIdAsync(int locationId, CancellationToken cancellationToken = default);

    /// <summary>Tìm MapLocation đang được gán cho ShopInfo này.</summary>
    Task<MapLocation?> GetLocationByShopInfoIdAsync(string shopInfoId, CancellationToken cancellationToken = default);

    /// <summary>Cập nhật Status và ShopInfoId của một MapLocation.</summary>
    Task<bool> UpdateLocationSlotAsync(int locationId, string status, string? shopInfoId, CancellationToken cancellationToken = default);

    /// <summary>Cập nhật chỉ Status của MapLocation đang liên kết với ShopInfo (dùng khi Manager khai trương).</summary>
    Task<bool> UpdateLocationStatusByShopInfoIdAsync(string shopInfoId, string status, CancellationToken cancellationToken = default);
}
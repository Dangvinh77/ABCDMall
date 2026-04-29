using ABCDMall.Modules.UtilityMap.Application.DTOs.Maps;

namespace ABCDMall.Modules.UtilityMap.Application.Services.Maps;

public interface IMapCommandService
{
    // --- Hiện có ---
    Task CreateFloorPlanAsync(CreateFloorPlanRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> UpdateFloorPlanAsync(int id, UpdateFloorPlanRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> AddLocationAsync(int floorPlanId, CreateMapLocationRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteLocationAsync(int locationId, CancellationToken cancellationToken = default);

    // --- MỚI: Slot management ---

    /// <summary>
    /// Admin gán slot cho một ShopInfo — đặt Status = "Reserved".
    /// Trả về false nếu slot không tồn tại hoặc đã bị chiếm.
    /// </summary>
    Task<bool> ReserveSlotAsync(int locationId, string shopInfoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Admin giải phóng slot (khi huỷ tenant) — đặt Status = "Available", ShopInfoId = null.
    /// </summary>
    Task<bool> ReleaseSlotAsync(int locationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật Status của slot theo ShopInfoId (khi Manager khai trương/cập nhật ngày).
    /// Dùng bởi ShopsController sau khi Manager cập nhật shop profile.
    /// </summary>
    Task UpdateSlotStatusByShopInfoIdAsync(string shopInfoId, string status, CancellationToken cancellationToken = default);
}
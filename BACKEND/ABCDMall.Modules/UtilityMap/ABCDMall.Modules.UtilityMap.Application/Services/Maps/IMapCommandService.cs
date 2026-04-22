using ABCDMall.Modules.UtilityMap.Application.DTOs.Maps;

namespace ABCDMall.Modules.UtilityMap.Application.Services.Maps;

public interface IMapCommandService
{
    Task CreateFloorPlanAsync(CreateFloorPlanRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> UpdateFloorPlanAsync(int id, UpdateFloorPlanRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> AddLocationAsync(int floorPlanId, CreateMapLocationRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteLocationAsync(int locationId, CancellationToken cancellationToken = default);
    Task<bool> ReserveSlotAsync(int locationId, string shopInfoId, CancellationToken cancellationToken = default);
    Task<bool> ReleaseSlotAsync(int locationId, CancellationToken cancellationToken = default);
    Task<bool> UpdateSlotStatusByShopInfoIdAsync(string shopInfoId, string status, CancellationToken cancellationToken = default);
}

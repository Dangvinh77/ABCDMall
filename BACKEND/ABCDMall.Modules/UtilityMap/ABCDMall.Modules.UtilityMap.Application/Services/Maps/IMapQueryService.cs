using ABCDMall.Modules.UtilityMap.Application.DTOs.Maps;

namespace ABCDMall.Modules.UtilityMap.Application.Services.Maps;

public interface IMapQueryService
{
    // --- Hiện có (public view) ---
    Task<IReadOnlyList<FloorPlanDto>> GetAllFloorsAsync(CancellationToken cancellationToken = default);
    Task<FloorPlanDto?> GetFloorPlanAsync(string floorLevel, CancellationToken cancellationToken = default);

    // --- MỚI: Admin view — thấy đủ tất cả slot kể cả "Available" ---
    Task<IReadOnlyList<FloorPlanAdminDto>> GetAllFloorsForAdminAsync(CancellationToken cancellationToken = default);
    Task<FloorPlanAdminDto?> GetFloorPlanForAdminAsync(string floorLevel, CancellationToken cancellationToken = default);
}
using ABCDMall.Modules.UtilityMap.Application.DTOs.Maps;

namespace ABCDMall.Modules.UtilityMap.Application.Services.Maps;

public interface IMapQueryService
{
    Task<IReadOnlyList<FloorPlanDto>> GetAllFloorsAsync(CancellationToken cancellationToken = default);
    Task<FloorPlanDto?> GetFloorPlanAsync(string floorLevel, CancellationToken cancellationToken = default);
}

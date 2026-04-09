using ABCDMall.Modules.UtilityMap.Domain.Entities;
using ABCDMall.Modules.UtilityMap.Domain.Interfaces;

namespace ABCDMall.Modules.UtilityMap.Application.Services;

public class MapService : IMapService
{
    private readonly IMapRepository _mapRepository;

    public MapService(IMapRepository mapRepository)
    {
        _mapRepository = mapRepository;
    }

    public async Task<FloorPlan?> GetFloorPlanAsync(string floorLevel)
    {
        // Ở đây tương lai bạn có thể thêm logic: kiểm tra cache, format lại dữ liệu...
        return await _mapRepository.GetFloorPlanAsync(floorLevel);
    }

    public async Task<List<FloorPlan>> GetAllFloorsAsync()
    {
        return await _mapRepository.GetAllFloorsAsync();
    }
}
using Microsoft.AspNetCore.Mvc;
using ABCDMall.Modules.UtilityMap.Domain.Interfaces;
using ABCDMall.Modules.UtilityMap.Domain.Entities;

namespace ABCDMall.WebAPI.Controllers.UtilityMap;

[ApiController]
[Route("api/map")] // Đường dẫn gốc sẽ là: localhost:xxxx/api/map
public class MapController : ControllerBase
{
    private readonly IMapService _mapService;

    public MapController(IMapService mapService)
    {
        _mapService = mapService;
    }

    // API lấy danh sách TẤT CẢ CÁC TẦNG
    // GET: /api/map/floors
    [HttpGet("floors")]
    public async Task<IActionResult> GetAllFloors()
    {
        var floors = await _mapService.GetAllFloorsAsync();
        return Ok(floors);
    }

    // API lấy chi tiết 1 TẦNG cụ thể
    // GET: /api/map/floors/L1
    [HttpGet("floors/{floorLevel}")]
    public async Task<IActionResult> GetFloorPlan(string floorLevel)
    {
        var floorPlan = await _mapService.GetFloorPlanAsync(floorLevel);
        
        if (floorPlan == null)
        {
            return NotFound(new { message = $"Không tìm thấy dữ liệu sơ đồ cho tầng {floorLevel}" });
        }
        
        return Ok(floorPlan);
    }
}
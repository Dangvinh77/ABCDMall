using Microsoft.AspNetCore.Mvc;
using ABCDMall.Modules.UtilityMap.Domain.Interfaces;
using ABCDMall.Shared.DTOs;

namespace ABCDMall.WebAPI.Controllers.UtilityMap;

[ApiController]
[Route("api/map")]
public class MapController : ControllerBase
{
    private readonly IMapService _service;

    public MapController(IMapService service)
    {
        _service = service;
    }

    // GET tất cả tầng + tọa độ shop
    [HttpGet("floors")]
    public async Task<IActionResult> GetAllFloors()
        => Ok(await _service.GetAllFloorsAsync());

    // GET 1 tầng theo level (L1, L3, L5, L6)
    [HttpGet("floors/{floorLevel}")]
    public async Task<IActionResult> GetFloor(string floorLevel)
    {
        var floor = await _service.GetFloorPlanAsync(floorLevel);
        if (floor == null) return NotFound();
        return Ok(floor);
    }

    // POST tạo tầng mới
    [HttpPost("floors")]
    public async Task<IActionResult> CreateFloor([FromBody] FloorPlanDto dto)
    {
        await _service.CreateFloorPlanAsync(dto);
        return Ok("Tạo tầng thành công");
    }

    // POST thêm điểm shop vào tầng
    [HttpPost("floors/{floorPlanId:int}/locations")]
    public async Task<IActionResult> AddLocation(int floorPlanId, [FromBody] MapLocationDto dto)
    {
        var result = await _service.AddLocationAsync(floorPlanId, dto);
        if (!result) return NotFound("Không tìm thấy tầng");
        return Ok("Thêm vị trí shop thành công");
    }
    // PUT cập nhật blueprint image URL của tầng
    [HttpPut("floors/{id:int}")]
    public async Task<IActionResult> UpdateFloor(int id, [FromBody] FloorPlanDto dto)
    {
        var result = await _service.UpdateFloorPlanAsync(id, dto);
        if (!result) return NotFound();
        return Ok("Cập nhật thành công");
    }
    // DELETE xóa điểm shop
    [HttpDelete("locations/{locationId:int}")]
    public async Task<IActionResult> DeleteLocation(int locationId)
    {
        var result = await _service.DeleteLocationAsync(locationId);
        if (!result) return NotFound();
        return Ok("Đã xóa vị trí shop");
    }
}
using ABCDMall.Modules.UtilityMap.Application.DTOs.Maps;
using ABCDMall.Modules.UtilityMap.Application.Services.Maps;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers;

[ApiController]
[Route("api/map/floors")]
public class MapsController : ControllerBase
{
    private readonly IMapQueryService _queryService;
    private readonly IMapCommandService _commandService;

    public MapsController(IMapQueryService queryService, IMapCommandService commandService)
    {
        _queryService = queryService;
        _commandService = commandService;
    }

    // ── PUBLIC ENDPOINTS ──────────────────────────────────────────────────

    /// <summary>Lấy tất cả tầng và locations — public, không thấy slot Available.</summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAllFloors(CancellationToken cancellationToken)
    {
        var result = await _queryService.GetAllFloorsAsync(cancellationToken);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("{floorLevel}")]
    public async Task<IActionResult> GetFloorPlan(string floorLevel, CancellationToken cancellationToken)
    {
        var result = await _queryService.GetFloorPlanAsync(floorLevel, cancellationToken);
        if (result is null) return NotFound();
        return Ok(result);
    }

    // ── ADMIN ENDPOINTS — Bản đồ nội bộ ─────────────────────────────────

    /// <summary>
    /// Admin view — thấy tất cả slot kể cả "Available" (vị trí trống màu bạc trên map).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public async Task<IActionResult> GetAllFloorsForAdmin(CancellationToken cancellationToken)
    {
        var result = await _queryService.GetAllFloorsForAdminAsync(cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/{floorLevel}")]
    public async Task<IActionResult> GetFloorPlanForAdmin(string floorLevel, CancellationToken cancellationToken)
    {
        var result = await _queryService.GetFloorPlanForAdminAsync(floorLevel, cancellationToken);
        if (result is null) return NotFound();
        return Ok(result);
    }

    // ── ADMIN SLOT MANAGEMENT ─────────────────────────────────────────────

    /// <summary>
    /// Admin gán slot cho một Manager (sau khi tạo tài khoản).
    /// Đặt Status = "Reserved", link ShopInfoId.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("locations/{locationId}/reserve")]
    public async Task<IActionResult> ReserveSlot(
        int locationId,
        [FromBody] AssignSlotRequestDto dto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.ShopInfoId))
        {
            return BadRequest("ShopInfoId is required.");
        }

        var result = await _commandService.ReserveSlotAsync(locationId, dto.ShopInfoId, cancellationToken);
        if (!result)
        {
            return BadRequest("Cannot reserve this slot. It may not exist or is already taken.");
        }

        return Ok(new { message = "Slot reserved successfully." });
    }

    /// <summary>
    /// Admin giải phóng slot (huỷ tenant) — Status → "Available", ShopInfoId → null.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("locations/{locationId}/release")]
    public async Task<IActionResult> ReleaseSlot(int locationId, CancellationToken cancellationToken)
    {
        var result = await _commandService.ReleaseSlotAsync(locationId, cancellationToken);
        if (!result)
        {
            return BadRequest("Cannot release this slot. It may not exist or is already Available.");
        }

        return Ok(new { message = "Slot released successfully." });
    }

    // ── ADMIN CRUD (đã có — thêm auth) ───────────────────────────────────

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateFloorPlan(
        [FromBody] CreateFloorPlanRequestDto dto,
        CancellationToken cancellationToken)
    {
        await _commandService.CreateFloorPlanAsync(dto, cancellationToken);
        return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFloorPlan(
        int id,
        [FromBody] UpdateFloorPlanRequestDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _commandService.UpdateFloorPlanAsync(id, dto, cancellationToken);
        if (!result) return NotFound();
        return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{floorPlanId}/locations")]
    public async Task<IActionResult> AddLocation(
        int floorPlanId,
        [FromBody] CreateMapLocationRequestDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _commandService.AddLocationAsync(floorPlanId, dto, cancellationToken);
        if (!result) return NotFound();
        return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("locations/{locationId}")]
    public async Task<IActionResult> DeleteLocation(int locationId, CancellationToken cancellationToken)
    {
        var result = await _commandService.DeleteLocationAsync(locationId, cancellationToken);
        if (!result) return NotFound();
        return Ok();
    }
}
using ABCDMall.Modules.UtilityMap.Application.DTOs.Maps;
using ABCDMall.Modules.UtilityMap.Application.Services.Maps;
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

    [HttpGet]
    public async Task<IActionResult> GetAllFloors(CancellationToken cancellationToken)
    {
        var result = await _queryService.GetAllFloorsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{floorLevel}")]
    public async Task<IActionResult> GetFloorPlan(string floorLevel, CancellationToken cancellationToken)
    {
        var result = await _queryService.GetFloorPlanAsync(floorLevel, cancellationToken);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateFloorPlan([FromBody] CreateFloorPlanRequestDto dto, CancellationToken cancellationToken)
    {
        await _commandService.CreateFloorPlanAsync(dto, cancellationToken);
        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFloorPlan(int id, [FromBody] UpdateFloorPlanRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _commandService.UpdateFloorPlanAsync(id, dto, cancellationToken);
        if (!result) return NotFound();
        return Ok();
    }

    [HttpPost("{floorPlanId}/locations")]
    public async Task<IActionResult> AddLocation(int floorPlanId, [FromBody] CreateMapLocationRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _commandService.AddLocationAsync(floorPlanId, dto, cancellationToken);
        if (!result) return NotFound();
        return Ok();
    }

    [HttpDelete("locations/{locationId}")]
    public async Task<IActionResult> DeleteLocation(int locationId, CancellationToken cancellationToken)
    {
        var result = await _commandService.DeleteLocationAsync(locationId, cancellationToken);
        if (!result) return NotFound();
        return Ok();
    }
}

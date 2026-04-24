using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Application.DTOs.Common;
using ABCDMall.Modules.Users.Application.DTOs.RentalAreas;
using ABCDMall.Modules.Users.Application.Services.RentalAreas;
using ABCDMall.Modules.Users.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class RentalAreaController : ControllerBase
{
    private readonly IRentalAreaQueryService _rentalAreaQueryService;
    private readonly IRentalAreaCommandService _rentalAreaCommandService;

    public RentalAreaController(
        IRentalAreaQueryService rentalAreaQueryService,
        IRentalAreaCommandService rentalAreaCommandService)
    {
        _rentalAreaQueryService = rentalAreaQueryService;
        _rentalAreaCommandService = rentalAreaCommandService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RentalAreaResponseDto>>> GetRentalAreas()
        => Ok(await _rentalAreaQueryService.GetRentalAreasAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<RentalAreaDetailResponseDto>> GetRentalAreaDetail(string id)
    {
        var rentalArea = await _rentalAreaQueryService.GetRentalAreaDetailAsync(id);
        return rentalArea is null ? NotFound("Rental area does not exist") : Ok(rentalArea);
    }

    [HttpGet("check-manager/{cccd}")]
    public async Task<ActionResult<ManagerLookupResponseDto>> CheckManagerByCccd(string cccd)
    {
        if (string.IsNullOrWhiteSpace(cccd))
        {
            return BadRequest("CCCD is required");
        }

        var manager = await _rentalAreaQueryService.CheckManagerByCccdAsync(cccd);
        return manager is null ? NotFound("Manager with this CCCD does not exist") : Ok(manager);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRentalArea(CreateRentalAreaDto dto)
    {
        var result = await _rentalAreaCommandService.CreateRentalAreaAsync(dto);
        return result.Status switch
        {
            ApplicationResultStatus.Ok => Ok(new
            {
                message = result.Value!.Message,
                rentalArea = result.Value.RentalArea
            }),
            ApplicationResultStatus.BadRequest => BadRequest(result.Error),
            ApplicationResultStatus.NotFound => NotFound(result.Error),
            ApplicationResultStatus.Unauthorized => Unauthorized(result.Error),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpPut("{id}/register-tenant")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> RegisterTenant(string id, [FromForm] RegisterTenantDto dto)
        => FromResult(await _rentalAreaCommandService.RegisterTenantAsync(id, dto));

    [HttpPut("{id}/monthly-bill")]
    public async Task<IActionResult> UpdateMonthlyBill(string id, UpdateMonthlyBillDto dto)
        => FromResult(await _rentalAreaCommandService.UpdateMonthlyBillAsync(id, dto));

    [HttpPut("{id}/cancel-tenant")]
    public async Task<IActionResult> CancelTenant(string id)
        => FromResult(await _rentalAreaCommandService.CancelTenantAsync(id));

    private IActionResult FromResult(ApplicationResult<MessageResponseDto> result)
    {
        return result.Status switch
        {
            ApplicationResultStatus.Ok => Ok(new { message = result.Value!.Message }),
            ApplicationResultStatus.BadRequest => BadRequest(result.Error),
            ApplicationResultStatus.NotFound => NotFound(result.Error),
            ApplicationResultStatus.Unauthorized => Unauthorized(result.Error),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}

using ABCDMall.Modules.Movies.Application.DTOs.Promotions;
using ABCDMall.Modules.Movies.Application.Services.Promotions;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers;

[ApiController]
[Route("api/snack-combos")]
public sealed class SnackCombosController : ControllerBase
{
    private readonly ISnackComboQueryService _snackComboQueryService;

    public SnackCombosController(ISnackComboQueryService snackComboQueryService)
    {
        _snackComboQueryService = snackComboQueryService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SnackComboResponseDto>>> GetSnackCombos(
        CancellationToken cancellationToken = default)
    {
        // Endpoint nay hoan tat phan "frontend co the goi combo that" trong Day 3.
        var combos = await _snackComboQueryService.GetSnackCombosAsync(cancellationToken);
        return Ok(combos);
    }
}

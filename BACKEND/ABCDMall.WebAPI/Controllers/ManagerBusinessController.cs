using ABCDMall.Modules.Users.Application.DTOs.PublicCatalog;
using ABCDMall.Modules.Users.Application.Services.PublicCatalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ABCDMall.WebAPI.Controllers;

[Route("api/manager-business")]
[ApiController]
[Authorize(Roles = "Manager")]
public sealed class ManagerBusinessController : ControllerBase
{
    private readonly IManagerBusinessRouteService _managerBusinessRouteService;

    public ManagerBusinessController(IManagerBusinessRouteService managerBusinessRouteService)
    {
        _managerBusinessRouteService = managerBusinessRouteService;
    }

    [HttpGet("route")]
    public async Task<ActionResult<ManagerBusinessRouteDto>> GetRoute(CancellationToken cancellationToken = default)
    {
        var ownerShopId = User.FindFirstValue("shopId");
        if (string.IsNullOrWhiteSpace(ownerShopId))
        {
            return BadRequest("Manager account does not have a shop id.");
        }

        return Ok(await _managerBusinessRouteService.GetRouteAsync(ownerShopId, cancellationToken));
    }
}

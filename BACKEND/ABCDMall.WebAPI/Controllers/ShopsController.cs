using ABCDMall.Modules.Users.Application.DTOs.PublicCatalog;
using ABCDMall.Modules.Users.Application.Services.PublicCatalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ABCDMall.Modules.UtilityMap.Application.Services.Maps;

namespace ABCDMall.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ShopsController : ControllerBase
{
    private readonly IPublicShopCatalogService _shopCatalogService;
    private readonly IShopInfoPublicManagerService _shopManagerService;
    private readonly IMapCommandService _mapCommandService;
    
    public ShopsController(
        IPublicShopCatalogService shopCatalogService,
        IShopInfoPublicManagerService shopManagerService,
        IMapCommandService mapCommandService)   // <-- THÊM
    {
        _shopCatalogService = shopCatalogService;
        _shopManagerService = shopManagerService;
        _mapCommandService = mapCommandService; // <-- THÊM
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PublicShopDto>>> GetShops(CancellationToken cancellationToken = default)
        => Ok(await _shopCatalogService.GetShopsAsync(cancellationToken));

    [AllowAnonymous]
    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<PublicShopDto>> GetShopBySlug(string slug, CancellationToken cancellationToken = default)
    {
        var shop = await _shopCatalogService.GetShopBySlugAsync(slug, cancellationToken);
        return shop is null ? NotFound() : Ok(shop);
    }

    [AllowAnonymous]
    [HttpGet("{slug}")]
    public async Task<ActionResult<PublicShopDto>> GetShopBySlugAlias(string slug, CancellationToken cancellationToken = default)
    {
        var shop = await _shopCatalogService.GetShopBySlugAsync(slug, cancellationToken);
        return shop is null ? NotFound() : Ok(shop);
    }

    [Authorize(Roles = "Manager")]
    [HttpGet("manager")]
    public async Task<ActionResult<IReadOnlyList<PublicShopDto>>> GetMyShops(CancellationToken cancellationToken = default)
    {
        var ownerShopId = GetOwnerShopId();
        if (string.IsNullOrWhiteSpace(ownerShopId))
        {
            return BadRequest("Manager account does not have a shop id.");
        }

        return Ok(await _shopManagerService.GetMyShopsAsync(ownerShopId, cancellationToken));
    }

    [Authorize(Roles = "Manager")]
    [HttpGet("manager/creation-status")]
    public async Task<ActionResult<ShopCreationStatusDto>> GetMyShopCreationStatus(CancellationToken cancellationToken = default)
    {
        var ownerShopId = GetOwnerShopId();
        if (string.IsNullOrWhiteSpace(ownerShopId))
        {
            return BadRequest("Manager account does not have a shop id.");
        }

        try
        {
            return Ok(await _shopManagerService.GetCreationStatusAsync(ownerShopId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "Manager")]
    [HttpPost("manager")]
    public async Task<ActionResult<PublicShopDto>> CreateMyShop(
            [FromForm] UpsertShopInfoPublicRequestDto request,
            CancellationToken cancellationToken = default)
    {
        var ownerShopId = GetOwnerShopId();
        if (string.IsNullOrWhiteSpace(ownerShopId))
        {
            return BadRequest("Manager account does not have a shop id.");
        }

        try
        {
            var shop = await _shopManagerService.CreateMyShopAsync(ownerShopId, request, cancellationToken);

            // --- MỚI: Sync trạng thái slot trên bản đồ ---
            // ownerShopId là ShopInfo.Id được link trong MapLocation.ShopInfoId
            var mapStatus = ShopInfoPublicMapper.DeriveShopStatus(request.OpeningDate);
            await _mapCommandService.UpdateSlotStatusByShopInfoIdAsync(ownerShopId, mapStatus, cancellationToken);

            return CreatedAtAction(nameof(GetShopBySlug), new { slug = shop.Slug }, shop);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "Manager")]
    [HttpPut("manager/{id}")]
    public async Task<ActionResult<PublicShopDto>> UpdateMyShop(
        string id,
        [FromForm] UpsertShopInfoPublicRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var ownerShopId = GetOwnerShopId();
        if (string.IsNullOrWhiteSpace(ownerShopId))
        {
            return BadRequest("Manager account does not have a shop id.");
        }

        try
        {
            var shop = await _shopManagerService.UpdateMyShopAsync(ownerShopId, id, request, cancellationToken);
            if (shop is null) return NotFound();

            // --- MỚI: Sync trạng thái slot trên bản đồ ---
            var mapStatus = ShopInfoPublicMapper.DeriveShopStatus(request.OpeningDate);
            await _mapCommandService.UpdateSlotStatusByShopInfoIdAsync(ownerShopId, mapStatus, cancellationToken);

            return Ok(shop);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "Manager")]
    [HttpDelete("manager/{id}")]
    public async Task<IActionResult> DeleteMyShop(string id, CancellationToken cancellationToken = default)
    {
        var ownerShopId = GetOwnerShopId();
        if (string.IsNullOrWhiteSpace(ownerShopId))
        {
            return BadRequest("Manager account does not have a shop id.");
        }

        var deleted = await _shopManagerService.DeleteMyShopAsync(ownerShopId, id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    private string? GetOwnerShopId()
        => User.FindFirstValue("shopId");
}

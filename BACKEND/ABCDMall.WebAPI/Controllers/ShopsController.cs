using ABCDMall.Modules.Shops.Application.DTOs;
using ABCDMall.Modules.Shops.Application.Services.Catalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class ShopsController : ControllerBase
{
    private readonly IShopCatalogQueryService _shopCatalogService;

    public ShopsController(IShopCatalogQueryService shopCatalogService)
    {
        _shopCatalogService = shopCatalogService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ShopCatalogItemDto>>> GetShops(CancellationToken cancellationToken = default)
        => Ok(await _shopCatalogService.GetShopsAsync(cancellationToken));

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<ShopDetailDto>> GetShopBySlug(string slug, CancellationToken cancellationToken = default)
    {
        var shop = await _shopCatalogService.GetShopBySlugAsync(slug, cancellationToken);
        return shop is null ? NotFound() : Ok(shop);
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<ShopDetailDto>> GetShopBySlugAlias(string slug, CancellationToken cancellationToken = default)
    {
        var shop = await _shopCatalogService.GetShopBySlugAsync(slug, cancellationToken);
        return shop is null ? NotFound() : Ok(shop);
    }
}

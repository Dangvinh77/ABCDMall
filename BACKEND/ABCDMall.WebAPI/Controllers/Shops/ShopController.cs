using ABCDMall.Modules.Shops.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers.Shops;

[ApiController]
[Route("api/shops")]
public class ShopController : ControllerBase
{
    private readonly IShopService _shopService;

    public ShopController(IShopService shopService)
    {
        _shopService = shopService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllShops()
    {
        var shops = await _shopService.GetAllShopsAsync();
        return Ok(shops);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetShopBySlug(string slug)
    {
        var shop = await _shopService.GetShopBySlugAsync(slug);
        if (shop == null)
            return NotFound(new { Message = "Không tìm thấy cửa hàng này." });

        return Ok(shop);
    }
}
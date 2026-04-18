using ABCDMall.Modules.Users.Application.DTOs.ShopInfos;
using ABCDMall.Modules.Users.Application.Services.ShopInfos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ABCDMall.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Manager,Admin")]
    public class ShopInfoController : ControllerBase
    {
        private readonly IShopInfoQueryService _shopInfoQueryService;

        public ShopInfoController(IShopInfoQueryService shopInfoQueryService)
        {
            _shopInfoQueryService = shopInfoQueryService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ShopMonthlyBillResponseDto>>> GetShopInfos()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var shopId = User.FindFirstValue("shopId");

            if (role == "Manager")
            {
                if (string.IsNullOrWhiteSpace(shopId))
                    return Ok(Array.Empty<ShopMonthlyBillResponseDto>());
            }

            var bills = await _shopInfoQueryService.GetBillsAsync(role == "Manager" ? shopId : null);
            return Ok(bills);
        }

    }
}

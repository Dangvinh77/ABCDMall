using ABCDMall.Modules.Shops.Application.DTOs;

namespace ABCDMall.Modules.Shops.Application.Interfaces;

public interface IShopService
{
    Task<IEnumerable<ShopDto>> GetAllShopsAsync();
    Task<ShopDto?> GetShopBySlugAsync(string slug);
}
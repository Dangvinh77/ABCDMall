using ABCDMall.Modules.Shops.Domain.Entities;

namespace ABCDMall.Modules.Shops.Domain.Interfaces;

public interface IShopRepository
{
    Task<IEnumerable<Shop>> GetAllShopsAsync();
    Task<Shop?> GetShopBySlugAsync(string slug);
}
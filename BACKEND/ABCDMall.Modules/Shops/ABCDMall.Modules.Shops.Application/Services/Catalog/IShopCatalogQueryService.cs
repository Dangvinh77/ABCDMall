using ABCDMall.Modules.Shops.Application.DTOs;

namespace ABCDMall.Modules.Shops.Application.Services.Catalog;

public interface IShopCatalogQueryService
{
    Task<IReadOnlyList<ShopCatalogItemDto>> GetShopsAsync(CancellationToken cancellationToken = default);
    Task<ShopDetailDto?> GetShopBySlugAsync(string slug, CancellationToken cancellationToken = default);
}

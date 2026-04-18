using ABCDMall.Modules.Shops.Domain.Entities;

namespace ABCDMall.Modules.Shops.Application.Services.Catalog;

public interface IShopCatalogRepository
{
    Task<IReadOnlyList<Shop>> GetShopsAsync(CancellationToken cancellationToken = default);
    Task<Shop?> GetShopBySlugAsync(string slug, CancellationToken cancellationToken = default);
}

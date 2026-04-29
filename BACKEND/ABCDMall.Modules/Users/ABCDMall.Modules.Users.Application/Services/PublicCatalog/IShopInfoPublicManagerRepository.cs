using ABCDMall.Modules.Users.Domain.Entities;

namespace ABCDMall.Modules.Users.Application.Services.PublicCatalog;

public interface IShopInfoPublicManagerRepository
{
    Task<ShopInfo?> GetShopInfoByIdAsync(string shopId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ShopInfo>> GetManagedShopInfosAsync(string ownerShopId, CancellationToken cancellationToken = default);
    Task<ShopInfo?> GetManagedShopInfoByIdAsync(string ownerShopId, string shopInfoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PublicShopProduct>> GetProductsAsync(string shopId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PublicShopVoucher>> GetVouchersAsync(string shopId, CancellationToken cancellationToken = default);
    Task<bool> ExistsVisibleSlugAsync(string slug, string? excludedShopId = null, CancellationToken cancellationToken = default);
    Task<int> CountManagedPublicShopsAsync(string shopId, CancellationToken cancellationToken = default);
    Task<int> CountRentedAreasAsync(string shopId, string tenantName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ShopInfo>> GetRentedAreaLocationsAsync(string shopId, string tenantName, CancellationToken cancellationToken = default);
    Task AddShopInfoAsync(ShopInfo shopInfo, CancellationToken cancellationToken = default);
    Task UpsertCatalogShopAsync(PublicShop shop, CancellationToken cancellationToken = default);
    Task ReplaceProductsAsync(IEnumerable<string> shopIdsToClear, string targetShopId, IReadOnlyList<PublicShopProduct> products, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

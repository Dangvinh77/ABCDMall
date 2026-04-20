using ABCDMall.Modules.Shops.Domain.Entities;

namespace ABCDMall.Modules.Shops.Application.Services.Manager;

public interface IShopManagerRepository
{
    Task<IReadOnlyList<Shop>> GetShopsByOwnerAsync(string ownerShopId, CancellationToken cancellationToken = default);
    Task<Shop?> GetShopByIdAndOwnerAsync(string id, string ownerShopId, CancellationToken cancellationToken = default);
    Task<bool> ExistsSlugAsync(string slug, string? excludedShopId = null, CancellationToken cancellationToken = default);
    Task AddShopAsync(Shop shop, CancellationToken cancellationToken = default);
    void RemoveShop(Shop shop);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

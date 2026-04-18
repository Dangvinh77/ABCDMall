using ABCDMall.Modules.Users.Domain.Entities;

namespace ABCDMall.Modules.Users.Application.Services.PublicCatalog;

public interface IPublicShopCatalogReadRepository
{
    Task<IReadOnlyList<ShopInfo>> GetShopInfosAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RentalArea>> GetRentalAreasAsync(CancellationToken cancellationToken = default);
}


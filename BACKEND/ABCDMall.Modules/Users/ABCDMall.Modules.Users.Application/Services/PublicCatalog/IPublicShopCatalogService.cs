using ABCDMall.Modules.Users.Application.DTOs.PublicCatalog;

namespace ABCDMall.Modules.Users.Application.Services.PublicCatalog;

public interface IPublicShopCatalogService
{
    Task<IReadOnlyList<PublicShopDto>> GetShopsAsync(CancellationToken cancellationToken = default);
    Task<PublicShopDto?> GetShopBySlugAsync(string slug, CancellationToken cancellationToken = default);
}


using ABCDMall.Modules.Users.Application.DTOs.PublicCatalog;

namespace ABCDMall.Modules.Users.Application.Services.PublicCatalog;

public interface IShopInfoPublicManagerService
{
    Task<IReadOnlyList<PublicShopDto>> GetMyShopsAsync(string shopId, CancellationToken cancellationToken = default);
    Task<ShopCreationStatusDto> GetCreationStatusAsync(string shopId, CancellationToken cancellationToken = default);
    Task<PublicShopDto> CreateMyShopAsync(string shopId, UpsertShopInfoPublicRequestDto request, CancellationToken cancellationToken = default);
    Task<PublicShopDto?> UpdateMyShopAsync(string shopId, string requestedShopId, UpsertShopInfoPublicRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteMyShopAsync(string shopId, string requestedShopId, CancellationToken cancellationToken = default);
}

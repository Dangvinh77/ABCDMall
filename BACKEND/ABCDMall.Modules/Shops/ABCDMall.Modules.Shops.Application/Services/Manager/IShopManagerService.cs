using ABCDMall.Modules.Shops.Application.DTOs;

namespace ABCDMall.Modules.Shops.Application.Services.Manager;

public interface IShopManagerService
{
    Task<IReadOnlyList<ShopDetailDto>> GetMyShopsAsync(string ownerShopId, CancellationToken cancellationToken = default);
    Task<ShopDetailDto> CreateMyShopAsync(string ownerShopId, UpsertManagedShopRequestDto request, CancellationToken cancellationToken = default);
    Task<ShopDetailDto?> UpdateMyShopAsync(string ownerShopId, string shopId, UpsertManagedShopRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteMyShopAsync(string ownerShopId, string shopId, CancellationToken cancellationToken = default);
}

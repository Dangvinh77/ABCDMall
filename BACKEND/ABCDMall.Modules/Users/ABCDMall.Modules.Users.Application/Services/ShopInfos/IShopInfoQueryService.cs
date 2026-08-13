using ABCDMall.Modules.Users.Application.DTOs.ShopInfos;

namespace ABCDMall.Modules.Users.Application.Services.ShopInfos;

public interface IShopInfoQueryService
{
    Task<IReadOnlyList<ShopMonthlyBillResponseDto>> GetBillsAsync(string? shopId, CancellationToken cancellationToken = default);
    Task<ShopRentalInfoResponseDto?> GetRentalInfoAsync(string? shopId, CancellationToken cancellationToken = default);
}

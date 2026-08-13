using ABCDMall.Modules.Users.Domain.Entities;

namespace ABCDMall.Modules.Users.Application.Services.ShopInfos;

public interface IShopMonthlyBillReadRepository
{
    Task<IReadOnlyList<ShopMonthlyBill>> GetBillsAsync(string? shopId, CancellationToken cancellationToken = default);
    Task<ShopInfo?> GetRentalInfoAsync(string? shopId, CancellationToken cancellationToken = default);
}

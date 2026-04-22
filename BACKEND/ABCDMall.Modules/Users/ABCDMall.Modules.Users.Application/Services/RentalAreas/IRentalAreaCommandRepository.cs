using ABCDMall.Modules.Users.Domain.Entities;

namespace ABCDMall.Modules.Users.Application.Services.RentalAreas;

public interface IRentalAreaCommandRepository
{
    Task<bool> ExistsRentalAreaByCodeAsync(string normalizedAreaCode, CancellationToken cancellationToken = default);

    Task<RentalArea?> GetRentalAreaByIdAsync(string rentalAreaId, CancellationToken cancellationToken = default);

    Task AddRentalAreaAsync(RentalArea rentalArea, CancellationToken cancellationToken = default);

    Task<User?> GetManagerByCccdAsync(string normalizedCccd, CancellationToken cancellationToken = default);

    Task<ShopInfo?> GetShopInfoByManagerAsync(User manager, string normalizedCccd, CancellationToken cancellationToken = default);

    Task<ShopInfo?> GetShopInfoByRentalAreaAsync(string rentalLocation, string? tenantName, string? shopInfoId, CancellationToken cancellationToken = default);

    Task AddMonthlyBillAsync(ShopMonthlyBill monthlyBill, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

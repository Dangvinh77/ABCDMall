using ABCDMall.Modules.Users.Domain.Entities;

namespace ABCDMall.Modules.Users.Application.Services.RentalAreas;

public interface IRentalAreaReadRepository
{
    Task<IReadOnlyList<RentalArea>> GetRentalAreasAsync(CancellationToken cancellationToken = default);
    Task<User?> GetManagerByCccdAsync(string normalizedCccd, CancellationToken cancellationToken = default);
    Task<ShopInfo?> GetShopInfoByManagerAsync(User manager, string normalizedCccd, CancellationToken cancellationToken = default);
}

using ABCDMall.Modules.Users.Application.Services.RentalAreas;
using ABCDMall.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Users.Infrastructure;

public sealed class RentalAreaReadRepository : IRentalAreaReadRepository
{
    private readonly MallDbContext _context;

    public RentalAreaReadRepository(MallDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<RentalArea>> GetRentalAreasAsync(CancellationToken cancellationToken = default)
    {
        return await _context.RentalAreas
            .OrderBy(x => x.AreaCode)
            .ToListAsync(cancellationToken);
    }

    public Task<User?> GetManagerByCccdAsync(string normalizedCccd, CancellationToken cancellationToken = default)
    {
        return _context.Users.FirstOrDefaultAsync(
            x => x.CCCD == normalizedCccd && x.Role == "Manager",
            cancellationToken);
    }

    public async Task<ShopInfo?> GetShopInfoByManagerAsync(User manager, string normalizedCccd, CancellationToken cancellationToken = default)
    {
        var shopInfo = !string.IsNullOrWhiteSpace(manager.ShopId)
            ? await _context.ShopInfos.FirstOrDefaultAsync(x => x.Id == manager.ShopId, cancellationToken)
            : null;

        return shopInfo ?? await _context.ShopInfos.FirstOrDefaultAsync(x => x.CCCD == normalizedCccd, cancellationToken);
    }
}

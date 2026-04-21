using ABCDMall.Modules.Users.Application.Services.RentalAreas;
using ABCDMall.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Users.Infrastructure;

public sealed class RentalAreaCommandRepository : IRentalAreaCommandRepository
{
    private readonly MallDbContext _context;

    public RentalAreaCommandRepository(MallDbContext context)
    {
        _context = context;
    }

    public Task<bool> ExistsRentalAreaByCodeAsync(string normalizedAreaCode, CancellationToken cancellationToken = default)
        => _context.RentalAreas.AnyAsync(x => x.AreaCode.ToLower() == normalizedAreaCode, cancellationToken);

    public Task<RentalArea?> GetRentalAreaByIdAsync(string rentalAreaId, CancellationToken cancellationToken = default)
        => _context.RentalAreas.FirstOrDefaultAsync(x => x.Id == rentalAreaId, cancellationToken);

    public Task AddRentalAreaAsync(RentalArea rentalArea, CancellationToken cancellationToken = default)
        => _context.RentalAreas.AddAsync(rentalArea, cancellationToken).AsTask();

    public Task<User?> GetManagerByCccdAsync(string normalizedCccd, CancellationToken cancellationToken = default)
        => _context.Users.FirstOrDefaultAsync(x => x.CCCD == normalizedCccd && x.Role == "Manager", cancellationToken);

    public async Task<ShopInfo?> GetShopInfoByManagerAsync(User manager, string normalizedCccd, CancellationToken cancellationToken = default)
    {
        var shopInfo = !string.IsNullOrWhiteSpace(manager.ShopId)
            ? await _context.ShopInfos.FirstOrDefaultAsync(x => x.Id == manager.ShopId, cancellationToken)
            : null;

        return shopInfo ?? await _context.ShopInfos.FirstOrDefaultAsync(x => x.CCCD == normalizedCccd, cancellationToken);
    }

    public async Task<ShopInfo?> GetShopInfoByRentalAreaAsync(
        string rentalLocation,
        string? tenantName,
        string? shopInfoId,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(shopInfoId))
        {
            var linkedShopInfo = await _context.ShopInfos.FirstOrDefaultAsync(x => x.Id == shopInfoId, cancellationToken);
            if (linkedShopInfo is not null)
            {
                return linkedShopInfo;
            }
        }

        var shopInfo = await _context.ShopInfos
            .FirstOrDefaultAsync(x => x.RentalLocation == rentalLocation && x.ShopName == tenantName, cancellationToken);

        return shopInfo ?? await _context.ShopInfos.FirstOrDefaultAsync(x => x.RentalLocation == rentalLocation, cancellationToken);
    }

    public Task AddMonthlyBillAsync(ShopMonthlyBill monthlyBill, CancellationToken cancellationToken = default)
        => _context.ShopMonthlyBills.AddAsync(monthlyBill, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}

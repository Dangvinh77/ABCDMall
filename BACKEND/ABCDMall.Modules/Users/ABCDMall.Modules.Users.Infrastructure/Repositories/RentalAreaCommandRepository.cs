using ABCDMall.Modules.Users.Application.Services.RentalAreas;
using ABCDMall.Modules.Users.Domain.Entities;
using ABCDMall.Modules.UtilityMap.Infrastructure.Persistence.UtilityMap;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Users.Infrastructure;

public sealed class RentalAreaCommandRepository : IRentalAreaCommandRepository
{
    private readonly MallDbContext _context;
    private readonly UtilityMapDbContext _utilityMapContext;

    public RentalAreaCommandRepository(MallDbContext context, UtilityMapDbContext utilityMapContext)
    {
        _context = context;
        _utilityMapContext = utilityMapContext;
    }

    public Task<bool> ExistsRentalAreaByCodeAsync(string normalizedAreaCode, CancellationToken cancellationToken = default)
        => _utilityMapContext.MapLocations.AnyAsync(
            x => x.LocationSlot.ToLower() == normalizedAreaCode,
            cancellationToken);

    public async Task<RentalArea?> GetRentalAreaByIdAsync(string rentalAreaId, CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(rentalAreaId, out var mapLocationId))
        {
            return null;
        }

        var shopInfos = await _context.ShopInfos
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var shopInfosById = shopInfos
            .Where(x => x.Id != null)
            .ToDictionary(x => x.Id!, StringComparer.OrdinalIgnoreCase);

        var location = await _utilityMapContext.MapLocations
            .AsNoTracking()
            .Include(x => x.FloorPlan)
            .FirstOrDefaultAsync(x => x.Id == mapLocationId, cancellationToken);

        return location is null
            ? null
            : RentalAreaReadRepository.MapToRentalArea(location, RentalAreaReadRepository.ResolveShopInfo(location, shopInfosById, shopInfos));
    }

    public Task AddRentalAreaAsync(RentalArea rentalArea, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public async Task<bool> UpdateRentalAreaTenantAsync(
        string rentalAreaId,
        string status,
        string? shopInfoId,
        string? tenantName,
        CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(rentalAreaId, out var mapLocationId))
        {
            return false;
        }

        var location = await _utilityMapContext.MapLocations.FirstOrDefaultAsync(x => x.Id == mapLocationId, cancellationToken);
        if (location is null)
        {
            return false;
        }

        location.Status = status;
        location.ShopInfoId = shopInfoId;
        location.ShopName = tenantName ?? string.Empty;
        return true;
    }

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

    public Task<User?> GetManagerByShopInfoIdAsync(string shopInfoId, CancellationToken cancellationToken = default)
        => _context.Users.FirstOrDefaultAsync(x => x.ShopId == shopInfoId && x.Role == "Manager", cancellationToken);

    public Task AddMonthlyBillAsync(ShopMonthlyBill monthlyBill, CancellationToken cancellationToken = default)
        => _context.ShopMonthlyBills.AddAsync(monthlyBill, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => SaveAllChangesAsync(cancellationToken);

    private async Task SaveAllChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
        await _utilityMapContext.SaveChangesAsync(cancellationToken);
    }
}

using ABCDMall.Modules.Users.Application.Services.RentalAreas;
using ABCDMall.Modules.Users.Domain.Entities;
using ABCDMall.Modules.UtilityMap.Infrastructure.Persistence.UtilityMap;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Users.Infrastructure;

public sealed class RentalAreaCommandRepository : IRentalAreaCommandRepository
{
    private readonly MallDbContext _context;
    private readonly UtilityMapDbContext _utilityMapContext;
    private readonly Dictionary<string, RentalArea> _transientRentalAreas = new(StringComparer.OrdinalIgnoreCase);

    public RentalAreaCommandRepository(MallDbContext context, UtilityMapDbContext utilityMapContext)
    {
        _context = context;
        _utilityMapContext = utilityMapContext;
    }

    public Task<bool> ExistsRentalAreaByCodeAsync(string normalizedAreaCode, CancellationToken cancellationToken = default)
        => _context.RentalAreas.AnyAsync(x => x.AreaCode.ToLower() == normalizedAreaCode, cancellationToken);

    public async Task<RentalArea?> GetRentalAreaByIdAsync(string rentalAreaId, CancellationToken cancellationToken = default)
    {
        if (_transientRentalAreas.TryGetValue(rentalAreaId, out var cachedRentalArea))
        {
            return cachedRentalArea;
        }

        var existingRentalArea = await _context.RentalAreas.FirstOrDefaultAsync(x => x.Id == rentalAreaId, cancellationToken);
        if (existingRentalArea is not null)
        {
            return existingRentalArea;
        }

        if (!int.TryParse(rentalAreaId, out var mapLocationId))
        {
            return null;
        }

        var mapLocation = await _utilityMapContext.MapLocations
            .Include(x => x.FloorPlan)
            .FirstOrDefaultAsync(x => x.Id == mapLocationId, cancellationToken);

        if (mapLocation is null)
        {
            return null;
        }

        var bridgedRentalArea = new RentalArea
        {
            Id = rentalAreaId,
            AreaCode = mapLocation.LocationSlot,
            Floor = mapLocation.FloorPlan?.FloorLevel ?? string.Empty,
            AreaName = string.IsNullOrWhiteSpace(mapLocation.ShopName) ? $"Map Slot {mapLocation.LocationSlot}" : mapLocation.ShopName,
            Size = string.Empty,
            MonthlyRent = 0,
            Status = string.Equals(mapLocation.Status, "Available", StringComparison.OrdinalIgnoreCase) ? "Available" : "Rented",
            TenantName = string.IsNullOrWhiteSpace(mapLocation.ShopName) ? null : mapLocation.ShopName,
            ShopInfoId = mapLocation.ShopInfoId,
            CreatedAt = DateTime.UtcNow
        };

        _transientRentalAreas[rentalAreaId] = bridgedRentalArea;
        return bridgedRentalArea;
    }

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

    public Task AddShopInfoAsync(ShopInfo shopInfo, CancellationToken cancellationToken = default)
        => _context.ShopInfos.AddAsync(shopInfo, cancellationToken).AsTask();

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

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var changedRentalAreas = _context.ChangeTracker.Entries<RentalArea>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified)
            .Select(entry => entry.Entity)
            .ToList();

        changedRentalAreas.AddRange(_transientRentalAreas.Values);

        foreach (var rentalArea in changedRentalAreas)
        {
            if (!int.TryParse(rentalArea.Id, out var mapLocationId))
            {
                continue;
            }

            var mapLocation = await _utilityMapContext.MapLocations.FirstOrDefaultAsync(x => x.Id == mapLocationId, cancellationToken);
            if (mapLocation is null)
            {
                continue;
            }

            mapLocation.Status = string.Equals(rentalArea.Status, "Available", StringComparison.OrdinalIgnoreCase)
                ? "Available"
                : "Rented";
            mapLocation.ShopInfoId = rentalArea.ShopInfoId;
            mapLocation.ShopName = rentalArea.TenantName;
        }

        await _context.SaveChangesAsync(cancellationToken);
        await _utilityMapContext.SaveChangesAsync(cancellationToken);
    }
}

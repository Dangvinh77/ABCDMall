using ABCDMall.Modules.Users.Application.DTOs.RentalAreas;
using ABCDMall.Modules.Users.Application.Services.RentalAreas;
using ABCDMall.Modules.Users.Domain.Entities;
using ABCDMall.Modules.UtilityMap.Infrastructure.Persistence.UtilityMap;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Users.Infrastructure;

public sealed class RentalAreaReadRepository : IRentalAreaReadRepository
{
    private readonly MallDbContext _context;
    private readonly UtilityMapDbContext _utilityMapContext;

    public RentalAreaReadRepository(MallDbContext context, UtilityMapDbContext utilityMapContext)
    {
        _context = context;
        _utilityMapContext = utilityMapContext;
    }

    public async Task<IReadOnlyList<RentalArea>> GetRentalAreasAsync(CancellationToken cancellationToken = default)
    {
        var shopInfos = await _context.ShopInfos
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var shopInfosById = shopInfos
            .Where(x => x.Id != null)
            .ToDictionary(x => x.Id!, StringComparer.OrdinalIgnoreCase);

        var locations = await _utilityMapContext.MapLocations
            .AsNoTracking()
            .Include(x => x.FloorPlan)
            .OrderBy(x => x.LocationSlot)
            .ToListAsync(cancellationToken);

        return locations
            .Select(location => MapToRentalArea(location, ResolveShopInfo(location, shopInfosById, shopInfos)))
            .ToList();
    }

    public async Task<RentalAreaDetailResponseDto?> GetRentalAreaDetailByIdAsync(string rentalAreaId, CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(rentalAreaId, out var mapLocationId))
        {
            return null;
        }

        var location = await _utilityMapContext.MapLocations
            .AsNoTracking()
            .Include(x => x.FloorPlan)
            .FirstOrDefaultAsync(x => x.Id == mapLocationId, cancellationToken);

        if (location is null)
        {
            return null;
        }

        var shopInfos = await _context.ShopInfos
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var shopInfosById = shopInfos
            .Where(x => x.Id != null)
            .ToDictionary(x => x.Id!, StringComparer.OrdinalIgnoreCase);

        var shopInfo = ResolveShopInfo(location, shopInfosById, shopInfos);

        return MapToRentalAreaDetail(location, shopInfo);
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

    internal static RentalArea MapToRentalArea(
        Modules.UtilityMap.Domain.Entities.MapLocation location,
        ShopInfo? shopInfo)
    {
        var tenantName = !string.IsNullOrWhiteSpace(shopInfo?.ShopName)
            ? shopInfo.ShopName
            : string.IsNullOrWhiteSpace(location.ShopName)
                ? null
                : location.ShopName;

        return new RentalArea
        {
            Id = location.Id.ToString(),
            AreaCode = location.LocationSlot,
            Floor = location.FloorPlan?.FloorLevel ?? string.Empty,
            AreaName = !string.IsNullOrWhiteSpace(location.ShopName)
                ? location.ShopName
                : $"Map Slot {location.LocationSlot}",
            Size = string.Empty,
            MonthlyRent = 0,
            Status = string.Equals(location.Status, "Available", StringComparison.OrdinalIgnoreCase) ? "Available" : "Rented",
            TenantName = tenantName,
            ShopInfoId = location.ShopInfoId,
            CreatedAt = DateTime.UtcNow,
            RemainingLeaseDays = CalculateRemainingLeaseDays(shopInfo),
            RemainingLeaseLabel = FormatRemainingLeaseLabel(shopInfo)
        };
    }

    internal static ShopInfo? ResolveShopInfo(
        Modules.UtilityMap.Domain.Entities.MapLocation location,
        IReadOnlyDictionary<string, ShopInfo> shopInfosById,
        IReadOnlyList<ShopInfo> shopInfos)
    {
        if (!string.IsNullOrWhiteSpace(location.ShopInfoId)
            && shopInfosById.TryGetValue(location.ShopInfoId, out var matchedById))
        {
            return matchedById;
        }

        var matchedByLocation = shopInfos.FirstOrDefault(x =>
            !string.IsNullOrWhiteSpace(x.RentalLocation)
            && string.Equals(x.RentalLocation, location.LocationSlot, StringComparison.OrdinalIgnoreCase));

        if (matchedByLocation is not null)
        {
            return matchedByLocation;
        }

        return shopInfos.FirstOrDefault(x =>
            !string.IsNullOrWhiteSpace(x.ShopName)
            && !string.IsNullOrWhiteSpace(location.ShopName)
            && string.Equals(x.ShopName, location.ShopName, StringComparison.OrdinalIgnoreCase));
    }

    private static RentalAreaDetailResponseDto MapToRentalAreaDetail(
        Modules.UtilityMap.Domain.Entities.MapLocation location,
        ShopInfo? shopInfo)
    {
        var tenantName = !string.IsNullOrWhiteSpace(shopInfo?.ShopName)
            ? shopInfo.ShopName
            : string.IsNullOrWhiteSpace(location.ShopName)
                ? null
                : location.ShopName;

        return new RentalAreaDetailResponseDto
        {
            Id = location.Id.ToString(),
            AreaCode = location.LocationSlot,
            Floor = location.FloorPlan?.FloorLevel ?? string.Empty,
            AreaName = !string.IsNullOrWhiteSpace(location.ShopName)
                ? location.ShopName
                : $"Map Slot {location.LocationSlot}",
            Size = string.Empty,
            MonthlyRent = 0,
            Status = string.Equals(location.Status, "Available", StringComparison.OrdinalIgnoreCase) ? "Available" : "Rented",
            TenantName = tenantName,
            ShopInfoId = location.ShopInfoId,
            ManagerName = shopInfo?.ManagerName,
            CCCD = shopInfo?.CCCD,
            ShopName = shopInfo?.ShopName,
            RentalLocation = shopInfo?.RentalLocation,
            LeaseStartDate = shopInfo is null || shopInfo.LeaseStartDate == default
                ? null
                : shopInfo.LeaseStartDate.ToString("yyyy-MM-dd"),
            ElectricityFee = shopInfo?.ElectricityFee ?? 0,
            WaterFee = shopInfo?.WaterFee ?? 0,
            ServiceFee = shopInfo?.ServiceFee ?? 0,
            LeaseTermDays = shopInfo?.LeaseTermDays ?? 0,
            ContractImage = shopInfo?.ContractImage,
            ContractImages = shopInfo?.ContractImages
        };
    }

    private static int? CalculateRemainingLeaseDays(ShopInfo? shopInfo)
    {
        if (shopInfo is null || shopInfo.LeaseStartDate == default || shopInfo.LeaseTermDays <= 0)
        {
            return null;
        }

        var leaseEndDate = shopInfo.LeaseStartDate.Date.AddDays(shopInfo.LeaseTermDays);
        return Math.Max(0, (leaseEndDate - DateTime.Today).Days);
    }

    private static string? FormatRemainingLeaseLabel(ShopInfo? shopInfo)
    {
        var remainingDays = CalculateRemainingLeaseDays(shopInfo);
        if (remainingDays is null)
        {
            return null;
        }

        return remainingDays.Value == 0 ? "Expired" : $"{remainingDays.Value} days left";
    }
}

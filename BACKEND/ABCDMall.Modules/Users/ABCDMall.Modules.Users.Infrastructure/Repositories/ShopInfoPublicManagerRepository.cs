using ABCDMall.Modules.Users.Application.Services.PublicCatalog;
using ABCDMall.Modules.Users.Application.DTOs.PublicCatalog;
using ABCDMall.Modules.Users.Domain.Entities;
using ABCDMall.Modules.UtilityMap.Infrastructure.Persistence.UtilityMap;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Users.Infrastructure.Repositories;

public sealed class ShopInfoPublicManagerRepository : IShopInfoPublicManagerRepository
{
    private readonly MallDbContext _context;
    private readonly UtilityMapDbContext _utilityMapContext;

    public ShopInfoPublicManagerRepository(MallDbContext context, UtilityMapDbContext utilityMapContext)
    {
        _context = context;
        _utilityMapContext = utilityMapContext;
    }

    public Task<ShopInfo?> GetShopInfoByIdAsync(string shopId, CancellationToken cancellationToken = default)
        => _context.ShopInfos.FirstOrDefaultAsync(x => x.Id == shopId, cancellationToken);

    public async Task<IReadOnlyList<ShopInfo>> GetManagedShopInfosAsync(string ownerShopId, CancellationToken cancellationToken = default)
        => await _context.ShopInfos
            .Where(x => x.Id == ownerShopId || x.OwnerShopInfoId == ownerShopId)
            .OrderBy(x => x.ShopName)
            .ToListAsync(cancellationToken);

    public Task<ShopInfo?> GetManagedShopInfoByIdAsync(string ownerShopId, string shopInfoId, CancellationToken cancellationToken = default)
        => _context.ShopInfos.FirstOrDefaultAsync(
            x => x.Id == shopInfoId && (x.Id == ownerShopId || x.OwnerShopInfoId == ownerShopId),
            cancellationToken);

    public async Task<IReadOnlyList<PublicShopProduct>> GetProductsAsync(string shopId, CancellationToken cancellationToken = default)
        => await _context.PublicShopProducts
            .AsNoTracking()
            .Where(x => x.ShopId == shopId)
            .OrderByDescending(x => x.IsFeatured)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<PublicShopVoucher>> GetVouchersAsync(string shopId, CancellationToken cancellationToken = default)
        => await _context.PublicShopVouchers
            .AsNoTracking()
            .Where(x => x.ShopId == shopId)
            .OrderByDescending(x => x.IsActive)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistsVisibleSlugAsync(string slug, string? excludedShopId = null, CancellationToken cancellationToken = default)
    {
        var shopInfoExists = await _context.ShopInfos.AnyAsync(
            x => x.IsPublicVisible && x.Slug == slug && (excludedShopId == null || x.Id != excludedShopId),
            cancellationToken);

        return shopInfoExists;
    }

    public Task<int> CountManagedPublicShopsAsync(string shopId, CancellationToken cancellationToken = default)
        => _context.ShopInfos.CountAsync(
            x => x.IsPublicVisible && (x.Id == shopId || x.OwnerShopInfoId == shopId),
            cancellationToken);

    public Task<int> CountRentedAreasAsync(string shopId, string tenantName, CancellationToken cancellationToken = default)
        => _utilityMapContext.MapLocations.CountAsync(
            x => x.Status != "Available" && (x.ShopInfoId == shopId || (x.ShopInfoId == null && x.ShopName == tenantName)),
            cancellationToken);

    public async Task<IReadOnlyList<AvailableRentalLocationDto>> GetAvailableRentalLocationsAsync(string shopId, string tenantName, CancellationToken cancellationToken = default)
    {
        var assignedLocations = await _utilityMapContext.MapLocations
            .AsNoTracking()
            .Include(x => x.FloorPlan)
            .Where(x => x.Status != "Available" && (x.ShopInfoId == shopId || (x.ShopInfoId == null && x.ShopName == tenantName)))
            .ToListAsync(cancellationToken);

        var usedLocationSlots = await _context.ShopInfos
            .AsNoTracking()
            .Where(x => x.IsPublicVisible && (x.Id == shopId || x.OwnerShopInfoId == shopId))
            .Select(x => x.LocationSlot)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToListAsync(cancellationToken);

        return assignedLocations
            .Select(x => new AvailableRentalLocationDto
            {
                LocationSlot = x.LocationSlot,
                Floor = string.IsNullOrWhiteSpace(x.FloorPlan?.FloorLevel)
                    ? ResolveFloorFromLocationSlot(x.LocationSlot)
                    : $"Floor {x.FloorPlan.FloorLevel.Trim()}",
                AreaName = x.ShopName
            })
            .Where(x => !usedLocationSlots.Contains(x.LocationSlot, StringComparer.OrdinalIgnoreCase))
            .GroupBy(x => $"{x.Floor}|{x.LocationSlot}", StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(x => x.Floor)
            .ThenBy(x => x.LocationSlot)
            .ToList();
    }

    private static string ResolveFloorFromLocationSlot(string? locationSlot)
    {
        var value = locationSlot?.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var digits = new string(value.TakeWhile(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(digits) ? value : $"Floor {digits}";
    }

    public Task AddShopInfoAsync(ShopInfo shopInfo, CancellationToken cancellationToken = default)
        => _context.ShopInfos.AddAsync(shopInfo, cancellationToken).AsTask();

    public async Task UpsertCatalogShopAsync(PublicShop shop, CancellationToken cancellationToken = default)
    {
        var existing = await _context.PublicShops.FirstOrDefaultAsync(x => x.Id == shop.Id, cancellationToken);
        if (existing is null)
        {
            _context.PublicShops.Add(shop);
            return;
        }

        existing.OwnerShopId = shop.OwnerShopId;
        existing.Name = shop.Name;
        existing.Slug = shop.Slug;
        existing.Category = shop.Category;
        existing.Floor = shop.Floor;
        existing.LocationSlot = shop.LocationSlot;
        existing.Summary = shop.Summary;
        existing.Description = shop.Description;
        existing.LogoUrl = shop.LogoUrl;
        existing.CoverImageUrl = shop.CoverImageUrl;
        existing.OpenHours = shop.OpenHours;
        existing.Badge = shop.Badge;
        existing.Offer = shop.Offer;
        existing.ShopStatus = shop.ShopStatus;
        existing.OpeningDate = shop.OpeningDate;
    }

    public async Task ReplaceProductsAsync(
        IEnumerable<string> shopIdsToClear,
        string targetShopId,
        IReadOnlyList<PublicShopProduct> products,
        CancellationToken cancellationToken = default)
    {
        var idsToClear = shopIdsToClear
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (idsToClear.Length > 0)
        {
            var existingProducts = await _context.PublicShopProducts
                .Where(x => idsToClear.Contains(x.ShopId))
                .ToListAsync(cancellationToken);

            _context.PublicShopProducts.RemoveRange(existingProducts);
        }

        foreach (var product in products)
        {
            product.ShopId = targetShopId;
            _context.PublicShopProducts.Add(product);
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}

using ABCDMall.Modules.Users.Application.Services.PublicCatalog;
using ABCDMall.Modules.Users.Domain.Entities;
using ABCDMall.Modules.UtilityMap.Infrastructure.Persistence.UtilityMap;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Users.Infrastructure.Repositories;

public sealed class PublicShopCatalogReadRepository : IPublicShopCatalogReadRepository
{
    private readonly MallDbContext _context;
    private readonly UtilityMapDbContext _utilityMapContext;

    public PublicShopCatalogReadRepository(MallDbContext context, UtilityMapDbContext utilityMapContext)
    {
        _context = context;
        _utilityMapContext = utilityMapContext;
    }

    public async Task<IReadOnlyList<ShopInfo>> GetShopInfosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ShopInfos
            .AsNoTracking()
            .OrderBy(x => x.ShopName)
            .ToListAsync(cancellationToken);
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
            .Select(location => RentalAreaReadRepository.MapToRentalArea(
                location,
                RentalAreaReadRepository.ResolveShopInfo(location, shopInfosById, shopInfos)))
            .ToList();
    }

    public async Task<IReadOnlyList<PublicShopProduct>> GetProductsAsync(IEnumerable<string> shopIds, CancellationToken cancellationToken = default)
    {
        var ids = shopIds.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        return await _context.PublicShopProducts
            .AsNoTracking()
            .Where(x => ids.Contains(x.ShopId))
            .OrderByDescending(x => x.IsFeatured)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PublicShopVoucher>> GetVouchersAsync(IEnumerable<string> shopIds, CancellationToken cancellationToken = default)
    {
        var ids = shopIds.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        return await _context.PublicShopVouchers
            .AsNoTracking()
            .Where(x => ids.Contains(x.ShopId))
            .OrderByDescending(x => x.IsActive)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);
    }
}


using ABCDMall.Modules.Users.Application.Services.PublicCatalog;
using ABCDMall.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Users.Infrastructure.Repositories;

public sealed class PublicShopCatalogReadRepository : IPublicShopCatalogReadRepository
{
    private readonly MallDbContext _context;

    public PublicShopCatalogReadRepository(MallDbContext context)
    {
        _context = context;
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
        return await _context.RentalAreas
            .AsNoTracking()
            .OrderBy(x => x.AreaCode)
            .ToListAsync(cancellationToken);
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


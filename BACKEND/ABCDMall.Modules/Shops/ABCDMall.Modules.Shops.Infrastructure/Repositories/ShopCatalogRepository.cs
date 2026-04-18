using ABCDMall.Modules.Shops.Application.Services.Catalog;
using ABCDMall.Modules.Shops.Domain.Entities;
using ABCDMall.Modules.Shops.Infrastructure.Persistence.Shops;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Shops.Infrastructure.Repositories;

public sealed class ShopCatalogRepository : IShopCatalogRepository
{
    private readonly ShopsDbContext _dbContext;

    public ShopCatalogRepository(ShopsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Shop>> GetShopsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Shops
            .AsNoTracking()
            .Include(x => x.Tags)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Shop?> GetShopBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Shops
            .AsNoTracking()
            .Include(x => x.Tags)
            .Include(x => x.Products)
            .Include(x => x.Vouchers)
            .FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);
    }
}

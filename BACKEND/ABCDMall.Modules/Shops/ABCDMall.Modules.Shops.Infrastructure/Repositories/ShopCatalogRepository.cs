using ABCDMall.Modules.Shops.Application.Services.Catalog;
using ABCDMall.Modules.Shops.Application.Services.Manager;
using ABCDMall.Modules.Shops.Domain.Entities;
using ABCDMall.Modules.Shops.Infrastructure.Persistence.Shops;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Shops.Infrastructure.Repositories;

public sealed class ShopCatalogRepository : IShopCatalogRepository, IShopManagerRepository
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

    public async Task<IReadOnlyList<Shop>> GetShopsByOwnerAsync(string ownerShopId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Shops
            .Include(x => x.Tags)
            .Include(x => x.Products)
            .Include(x => x.Vouchers)
            .Where(x => x.OwnerShopId == ownerShopId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Shop?> GetShopByIdAndOwnerAsync(string id, string ownerShopId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Shops
            .Include(x => x.Tags)
            .Include(x => x.Products)
            .Include(x => x.Vouchers)
            .FirstOrDefaultAsync(x => x.Id == id && x.OwnerShopId == ownerShopId, cancellationToken);
    }

    public Task<bool> ExistsSlugAsync(string slug, string? excludedShopId = null, CancellationToken cancellationToken = default)
    {
        return _dbContext.Shops.AnyAsync(
            x => x.Slug == slug && (excludedShopId == null || x.Id != excludedShopId),
            cancellationToken);
    }

    public async Task AddShopAsync(Shop shop, CancellationToken cancellationToken = default)
    {
        await _dbContext.Shops.AddAsync(shop, cancellationToken);
    }

    public void RemoveShop(Shop shop)
    {
        _dbContext.Shops.Remove(shop);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);
}

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
            .OrderBy(x => x.ShopName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RentalArea>> GetRentalAreasAsync(CancellationToken cancellationToken = default)
    {
        return await _context.RentalAreas
            .OrderBy(x => x.AreaCode)
            .ToListAsync(cancellationToken);
    }
}


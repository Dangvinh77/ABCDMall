using ABCDMall.Modules.Users.Application.Services.PublicCatalog;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Users.Infrastructure.Repositories;

public sealed class ManagerBusinessRouteRepository : IManagerBusinessRouteRepository
{
    private readonly MallDbContext _context;

    public ManagerBusinessRouteRepository(MallDbContext context)
    {
        _context = context;
    }

    public async Task<ManagerBusinessRouteSnapshot?> GetSnapshotAsync(string ownerShopId, CancellationToken cancellationToken = default)
    {
        var relatedShops = await _context.ShopInfos
            .AsNoTracking()
            .Where(x => x.Id == ownerShopId || x.OwnerShopInfoId == ownerShopId)
            .Select(x => new
            {
                x.Id,
                x.ShopName,
                x.RentalLocation
            })
            .ToListAsync(cancellationToken);

        var relatedShopIds = relatedShops
            .Select(x => x.Id)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (relatedShopIds.Count == 0)
        {
            relatedShopIds.Add(ownerShopId);
        }

        var match = await _context.RentalAreas
            .AsNoTracking()
            .Where(x => x.Status == "Rented"
                && !string.IsNullOrWhiteSpace(x.BusinessType)
                && x.ShopInfoId != null
                && relatedShopIds.Contains(x.ShopInfoId))
            .OrderByDescending(x => x.BusinessType == "FoodCourt")
            .ThenBy(x => x.AreaCode)
            .Select(x => new
            {
                x.BusinessType
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (match is null)
        {
            var relatedShopNames = relatedShops
                .Select(x => x.ShopName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var relatedRentalLocations = relatedShops
                .Select(x => x.RentalLocation)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            match = await _context.RentalAreas
                .AsNoTracking()
                .Where(x => x.Status == "Rented"
                    && !string.IsNullOrWhiteSpace(x.BusinessType)
                    && ((x.TenantName != null && relatedShopNames.Contains(x.TenantName))
                        || relatedRentalLocations.Contains(x.AreaCode)))
                .OrderByDescending(x => x.BusinessType == "FoodCourt")
                .ThenBy(x => x.AreaCode)
                .Select(x => new
                {
                    x.BusinessType
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (match is null || string.IsNullOrWhiteSpace(match.BusinessType))
        {
            return null;
        }

        return new ManagerBusinessRouteSnapshot
        {
            OwnerShopId = ownerShopId,
            BusinessType = match.BusinessType,
            HasEligibleRental = true
        };
    }
}

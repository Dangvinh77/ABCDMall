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
        var rentalAreas = await _context.RentalAreas
            .AsNoTracking()
            .Where(x => x.ShopInfoId == ownerShopId && x.Status == "Rented" && !string.IsNullOrWhiteSpace(x.BusinessType))
            .OrderByDescending(x => x.BusinessType == "FoodCourt")
            .ThenBy(x => x.AreaCode)
            .Select(x => new
            {
                x.ShopInfoId,
                x.BusinessType
            })
            .ToListAsync(cancellationToken);

        var match = rentalAreas.FirstOrDefault();
        if (match is null || string.IsNullOrWhiteSpace(match.ShopInfoId) || string.IsNullOrWhiteSpace(match.BusinessType))
        {
            return null;
        }

        return new ManagerBusinessRouteSnapshot
        {
            OwnerShopId = match.ShopInfoId,
            BusinessType = match.BusinessType,
            HasEligibleRental = true
        };
    }
}

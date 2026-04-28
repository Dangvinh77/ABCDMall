using ABCDMall.Modules.Users.Infrastructure;
using ABCDMall.Modules.Users.Infrastructure.Seed;
using ABCDMall.Modules.UtilityMap.Infrastructure.Persistence.UtilityMap;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ABCDMall.Modules.Users.Tests;

public class FrontendSeedIntegrationTests
{
    [Fact]
    public async Task FrontendSeeds_create_thirty_rented_manager_shops_aligned_with_utility_map()
    {
        await using var mallContext = CreateMallContext();
        await using var utilityMapContext = CreateUtilityMapContext();

        await FrontendUsersSeed.SeedAsync(mallContext);
        await ABCDMall.Modules.UtilityMap.Infrastructure.Seed.FrontendUtilityMapSeed.SeedAsync(utilityMapContext);

        var managerUsers = await mallContext.Users
            .Where(x => x.Role == "Manager")
            .ToListAsync();
        var assignedManagers = managerUsers
            .Where(x => !string.IsNullOrWhiteSpace(x.ShopId))
            .ToList();
        var unassignedManagers = managerUsers
            .Where(x => string.IsNullOrWhiteSpace(x.ShopId))
            .ToList();
        var managedShops = await mallContext.ShopInfos
            .Where(x => x.Id != null && x.Id.StartsWith("shop-"))
            .ToListAsync();
        var rentedMapLocations = await utilityMapContext.MapLocations
            .Where(x => x.Status == "Rented" && x.ShopInfoId != null)
            .ToListAsync();
        var billedShopIds = await mallContext.ShopMonthlyBills
            .Select(x => x.ShopInfoId)
            .Distinct()
            .ToListAsync();

        Assert.Equal(80, managerUsers.Count);
        Assert.Equal(30, assignedManagers.Count);
        Assert.Equal(50, unassignedManagers.Count);
        Assert.Equal(30, managedShops.Count(x => !string.IsNullOrWhiteSpace(x.RentalLocation)));
        Assert.Equal(30, billedShopIds.Count);
        Assert.Equal(30, rentedMapLocations.Count);

        foreach (var shop in managedShops)
        {
            Assert.Contains(
                rentedMapLocations,
                location => string.Equals(location.LocationSlot, shop.RentalLocation, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(location.ShopInfoId, shop.Id, StringComparison.OrdinalIgnoreCase));
        }
    }

    private static MallDbContext CreateMallContext()
    {
        var options = new DbContextOptionsBuilder<MallDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new MallDbContext(options);
    }

    private static UtilityMapDbContext CreateUtilityMapContext()
    {
        var options = new DbContextOptionsBuilder<UtilityMapDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new UtilityMapDbContext(options);
    }
}

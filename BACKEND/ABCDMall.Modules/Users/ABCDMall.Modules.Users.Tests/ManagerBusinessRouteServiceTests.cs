using ABCDMall.Modules.Users.Application.Services.PublicCatalog;
using Xunit;

namespace ABCDMall.Modules.Users.Tests;

public class ManagerBusinessRouteServiceTests
{
    [Fact]
    public async Task GetRouteAsync_returns_food_court_target_for_food_court_rental()
    {
        var repository = new FakeManagerBusinessRouteRepository
        {
            Snapshot = new ManagerBusinessRouteSnapshot
            {
                OwnerShopId = "shop-info-1",
                BusinessType = "FoodCourt",
                HasEligibleRental = true
            }
        };

        var service = new ManagerBusinessRouteService(repository);

        var result = await service.GetRouteAsync("shop-info-1");

        Assert.Equal("FoodCourt", result.BusinessType);
        Assert.Equal("/food-court-manager", result.TargetPath);
        Assert.True(result.HasEligibleRental);
    }

    [Fact]
    public async Task GetRouteAsync_returns_shop_target_for_shop_rental()
    {
        var repository = new FakeManagerBusinessRouteRepository
        {
            Snapshot = new ManagerBusinessRouteSnapshot
            {
                OwnerShopId = "shop-info-1",
                BusinessType = "Shop",
                HasEligibleRental = true
            }
        };

        var service = new ManagerBusinessRouteService(repository);

        var result = await service.GetRouteAsync("shop-info-1");

        Assert.Equal("Shop", result.BusinessType);
        Assert.Equal("/manager-shops", result.TargetPath);
        Assert.True(result.HasEligibleRental);
    }

    [Fact]
    public async Task GetRouteAsync_returns_rental_areas_when_no_eligible_rental_exists()
    {
        var repository = new FakeManagerBusinessRouteRepository();
        var service = new ManagerBusinessRouteService(repository);

        var result = await service.GetRouteAsync("shop-info-1");

        Assert.Equal(string.Empty, result.BusinessType);
        Assert.Equal("/rental-areas", result.TargetPath);
        Assert.False(result.HasEligibleRental);
    }

    private sealed class FakeManagerBusinessRouteRepository : IManagerBusinessRouteRepository
    {
        public ManagerBusinessRouteSnapshot? Snapshot { get; set; }

        public Task<ManagerBusinessRouteSnapshot?> GetSnapshotAsync(string ownerShopId, CancellationToken cancellationToken = default)
            => Task.FromResult(Snapshot);
    }
}

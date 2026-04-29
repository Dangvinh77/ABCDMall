using ABCDMall.Modules.Users.Domain.Entities;
using ABCDMall.Modules.Users.Infrastructure;
using ABCDMall.Modules.Users.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ABCDMall.Modules.Users.Tests;

public class ManagerBusinessRouteRepositoryTests
{
    [Fact]
    public async Task GetSnapshotAsync_returns_shop_route_snapshot_when_rental_is_linked_to_related_shop()
    {
        await using var context = CreateContext();
        context.ShopInfos.AddRange(
            new ShopInfo
            {
                Id = "shop-root",
                ShopName = "Root Manager Shop",
                RentalLocation = "A1"
            },
            new ShopInfo
            {
                Id = "shop-child",
                OwnerShopInfoId = "shop-root",
                ShopName = "Child Public Shop",
                RentalLocation = "A1"
            });
        context.RentalAreas.Add(new RentalArea
        {
            Id = "rental-1",
            AreaCode = "A1",
            Status = "Rented",
            ShopInfoId = "shop-child",
            BusinessType = "Shop"
        });
        await context.SaveChangesAsync();

        var repository = new ManagerBusinessRouteRepository(context);

        var snapshot = await repository.GetSnapshotAsync("shop-root");

        Assert.NotNull(snapshot);
        Assert.Equal("Shop", snapshot!.BusinessType);
        Assert.True(snapshot.HasEligibleRental);
    }

    private static MallDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MallDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new MallDbContext(options);
    }
}

using ABCDMall.Modules.FoodCourt.Infrastructure.Persistence.FoodCourt;
using ABCDMall.Modules.FoodCourt.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ABCDMall.Modules.FoodCourt.Tests;

public sealed class FrontendFoodCourtSeedTests
{
    [Fact]
    public async Task SeedAsync_UsesLocalMallBrandsAndImgAssets()
    {
        var options = new DbContextOptionsBuilder<FoodCourtDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        await using var db = new FoodCourtDbContext(options);

        await FrontendFoodCourtSeed.SeedAsync(db);

        var items = await db.FoodItems
            .OrderBy(item => item.Id)
            .ToListAsync();

        Assert.Collection(items,
            item =>
            {
                Assert.Equal("food-001", item.Id);
                Assert.Equal("Lok Lok Hotpot", item.Name);
                Assert.Equal("lok-lok-hotpot", item.Slug);
                Assert.Equal("/img/Lok Lok Hotpot/out.jpg", item.ImageUrl);
            },
            item =>
            {
                Assert.Equal("food-002", item.Id);
                Assert.Equal("Boba Bella Milk Tea", item.Name);
                Assert.Equal("boba-bella", item.Slug);
                Assert.Equal("/img/Boba Bella Milk Tea/menu.jpg", item.ImageUrl);
            },
            item =>
            {
                Assert.Equal("food-003", item.Id);
                Assert.Equal("Crystal Jade", item.Name);
                Assert.Equal("crystal-jade", item.Slug);
                Assert.Equal("/img/crystaljade/menu.webp", item.ImageUrl);
            },
            item =>
            {
                Assert.Equal("food-004", item.Id);
                Assert.Equal("King BBQ", item.Name);
                Assert.Equal("king-bbq", item.Slug);
                Assert.Equal("/img/King BBQ/out.jpg", item.ImageUrl);
            },
            item =>
            {
                Assert.Equal("food-005", item.Id);
                Assert.Equal("Marukame Udon", item.Name);
                Assert.Equal("marukame-udon", item.Slug);
                Assert.Equal("/img/Marukame Udon/out.jpg", item.ImageUrl);
            });
    }
}

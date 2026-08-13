using ABCDMall.Modules.FoodCourt.Infrastructure.Persistence.FoodCourt;
using ABCDMall.Modules.FoodCourt.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ABCDMall.Modules.FoodCourt.Tests;

public sealed class FrontendFoodCourtSeedTests
{
    [Fact]
    public async Task SeedAsync_UsesExpandedFoodCourtStoreCatalog()
    {
        var options = new DbContextOptionsBuilder<FoodCourtDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        await using var db = new FoodCourtDbContext(options);

        await FrontendFoodCourtSeed.SeedAsync(db);

        var items = await db.FoodItems
            .OrderBy(item => item.Id)
            .ToListAsync();

        Assert.Equal(38, items.Count);

        var starbucks = Assert.Single(items, item => item.Id == "food-001");
        Assert.Equal("STARBUCKS", starbucks.Name);
        Assert.Equal("starbucks", starbucks.Slug);
        Assert.Equal("/img/starbuck/logo.png", starbucks.ImageUrl);
        Assert.Equal("coffee", starbucks.CategorySlug);

        var crystalJade = Assert.Single(items, item => item.Id == "food-015");
        Assert.Equal("Crystal Jade", crystalJade.Name);
        Assert.Equal("crystal-jade", crystalJade.Slug);
        Assert.Equal("/img/crystaljade/logo.png", crystalJade.ImageUrl);

        var oceanBlue = Assert.Single(items, item => item.Id == "food-034");
        Assert.Equal("Ocean Blue Seafood Buffet", oceanBlue.Name);
        Assert.Equal("ocean-blue", oceanBlue.Slug);
        Assert.Contains("unsplash.com", oceanBlue.ImageUrl, StringComparison.OrdinalIgnoreCase);

        var tokyoRamen = Assert.Single(items, item => item.Id == "food-038");
        Assert.Equal("Tokyo Ramen Station", tokyoRamen.Name);
        Assert.Equal("tokyo-ramen-station", tokyoRamen.Slug);
        Assert.Equal("japanese", tokyoRamen.CategorySlug);
    }
}

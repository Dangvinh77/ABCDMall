using ABCDMall.Modules.Movies.Infrastructure.Persistence.Catalog;
using ABCDMall.Modules.Movies.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Movies.Tests;

internal static class CatalogSeedTestDb
{
    public static async Task<MoviesCatalogDbContext> CreateSeededContextAsync()
    {
        var options = new DbContextOptionsBuilder<MoviesCatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new MoviesCatalogDbContext(options);
        await FrontendMoviesSeed.SeedCatalogAsync(dbContext);
        return dbContext;
    }
}

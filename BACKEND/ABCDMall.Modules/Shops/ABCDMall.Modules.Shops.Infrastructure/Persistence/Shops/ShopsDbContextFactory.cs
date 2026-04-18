using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ABCDMall.Modules.Shops.Infrastructure.Persistence.Shops;

public sealed class ShopsDbContextFactory : IDesignTimeDbContextFactory<ShopsDbContext>
{
    public ShopsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ShopsDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\MSSQLLocalDB;Database=ABCDMall_Shops;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true",
            sql =>
            {
                sql.MigrationsAssembly(typeof(ShopsDbContext).Assembly.FullName);
                sql.MigrationsHistoryTable("__EFMigrationsHistory_Shops", ShopsDbContext.DefaultSchema);
            });

        return new ShopsDbContext(optionsBuilder.Options);
    }
}

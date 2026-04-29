using ABCDMall.Modules.Shops.Application.Services.Catalog;
using ABCDMall.Modules.Shops.Application.Services.Manager;
using ABCDMall.Modules.Shops.Infrastructure.Persistence.Shops;
using ABCDMall.Modules.Shops.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ABCDMall.Modules.Shops.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddShopsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ABCDMallConnection")
            ?? configuration.GetConnectionString("ABCDMallShopsDBConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'ABCDMallConnection' or 'ABCDMallShopsDBConnection' was not found.");

        services.AddDbContext<ShopsDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(ShopsDbContext).Assembly.FullName);
                sql.MigrationsHistoryTable("__EFMigrationsHistory_Shops", ShopsDbContext.DefaultSchema);
            });
        });

        services.AddScoped<IShopCatalogRepository, ShopCatalogRepository>();
        services.AddScoped<IShopManagerRepository, ShopCatalogRepository>();
        return services;
    }
}

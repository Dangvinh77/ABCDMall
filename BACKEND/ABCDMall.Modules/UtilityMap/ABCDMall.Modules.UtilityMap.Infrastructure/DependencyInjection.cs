using ABCDMall.Modules.UtilityMap.Application.Services.Maps;
using ABCDMall.Modules.UtilityMap.Infrastructure.Persistence.UtilityMap;
using ABCDMall.Modules.UtilityMap.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ABCDMall.Modules.UtilityMap.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUtilityMapInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ABCDMallConnection")
            ?? throw new InvalidOperationException("Connection string 'ABCDMallConnection' was not found.");

        services.AddDbContext<UtilityMapDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(UtilityMapDbContext).Assembly.FullName);
                sql.MigrationsHistoryTable(
                    "__EFMigrationsHistory_UtilityMap",
                    UtilityMapDbContext.DefaultSchema);
            });
        });

        services.AddScoped<IMapRepository, MapRepository>();
        return services;
    }
}

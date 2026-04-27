using ABCDMall.Modules.FoodCourt.Application.Services.Foods;
using ABCDMall.Modules.FoodCourt.Infrastructure.Persistence.FoodCourt;
using ABCDMall.Modules.FoodCourt.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ABCDMall.Modules.FoodCourt.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFoodCourtInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ABCDMallConnection")
            ?? configuration.GetConnectionString("ABCDMallFoodCourtDBConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'ABCDMallConnection' or 'ABCDMallFoodCourtDBConnection' was not found.");

        services.AddDbContext<FoodCourtDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(FoodCourtDbContext).Assembly.FullName);
                sql.MigrationsHistoryTable(
                    "__EFMigrationsHistory_FoodCourt",
                    null);
            });
        });

        services.AddScoped<IFoodRepository, FoodRepository>();
        return services;
    }
}

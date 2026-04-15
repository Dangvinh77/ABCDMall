using ABCDMall.Modules.Movies.Application.Services.Bookings;
using ABCDMall.Modules.Movies.Application.Services.Promotions;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Catalog;
using ABCDMall.Modules.Movies.Infrastructure.Repositories.Bookings;
using ABCDMall.Modules.Movies.Infrastructure.Repositories.Promotions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ABCDMall.Modules.Movies.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMoviesInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ABCDMallMoviesDBConnection")
            ?? throw new InvalidOperationException("Connection string 'ABCDMallMoviesDBConnection' was not found.");

        services.AddDbContext<MoviesCatalogDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(MoviesCatalogDbContext).Assembly.FullName);
                sql.MigrationsHistoryTable("__EFMigrationsHistory_MoviesCatalog", MoviesCatalogDbContext.DefaultSchema);
            });
        });

        services.AddDbContext<MoviesBookingDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(MoviesBookingDbContext).Assembly.FullName);
                sql.MigrationsHistoryTable("__EFMigrationsHistory_MoviesBooking", MoviesBookingDbContext.DefaultSchema);
            });
        });

        services.AddScoped<IPromotionRepository, PromotionRepository>();
        services.AddScoped<CatalogReadRepository>();
        services.AddScoped<IShowtimeReadRepository>(sp => sp.GetRequiredService<CatalogReadRepository>());
        services.AddScoped<ISeatInventoryReadRepository>(sp => sp.GetRequiredService<CatalogReadRepository>());

        return services;
    }
}

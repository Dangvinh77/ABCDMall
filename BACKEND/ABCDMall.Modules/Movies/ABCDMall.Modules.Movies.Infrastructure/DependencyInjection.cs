using ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking;
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
        var connectionString = configuration.GetConnectionString("MoviesBookingConnection")
            ?? throw new InvalidOperationException("Connection string 'MoviesBookingConnection' was not found.");

        services.AddDbContext<MoviesBookingDbContext>(options =>
        {
            options.UseSqlServer(
                connectionString,
                sql =>
                {
                    sql.MigrationsAssembly(typeof(MoviesBookingDbContext).Assembly.FullName);
                    sql.MigrationsHistoryTable("__EFMigrationsHistory", MoviesBookingDbContext.DefaultSchema);
                });
        });

        return services;
    }
}

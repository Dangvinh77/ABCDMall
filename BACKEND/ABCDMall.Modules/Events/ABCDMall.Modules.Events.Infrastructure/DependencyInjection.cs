using ABCDMall.Modules.Events.Application.Services.Events;
using ABCDMall.Modules.Events.Infrastructure.Persistence.Events;
using ABCDMall.Modules.Events.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ABCDMall.Modules.Events.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddEventsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("ABCDMallConnection")
            ?? configuration.GetConnectionString("ABCDMallEventsDBConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'ABCDMallConnection' or 'ABCDMallEventsDBConnection' was not found.");

        services.AddDbContext<EventsDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(EventsDbContext).Assembly.FullName);
                sql.MigrationsHistoryTable(
                    "__EFMigrationsHistory_Events",
                    EventsDbContext.DefaultSchema);
            });
        });

        services.AddScoped<IEventRepository, EventRepository>();

        return services;
    }
}
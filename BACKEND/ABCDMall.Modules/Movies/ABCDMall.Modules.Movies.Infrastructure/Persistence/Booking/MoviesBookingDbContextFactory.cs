using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking;

public class MoviesBookingDbContextFactory : IDesignTimeDbContextFactory<MoviesBookingDbContext>
{
    public MoviesBookingDbContext CreateDbContext(string[] args)
    {
        var currentDirectory = Directory.GetCurrentDirectory();

        var candidatePaths = new[]
        {
            Path.Combine(currentDirectory, "appsettings.json"),
            Path.Combine(currentDirectory, "ABCDMall.WebAPI", "appsettings.json"),
            Path.Combine(currentDirectory, "..", "..", "..", "..", "ABCDMall.WebAPI", "appsettings.json"),
            Path.Combine(currentDirectory, "..", "..", "..", "..", "..", "ABCDMall.WebAPI", "appsettings.json")
        };

        var appSettingsPath = candidatePaths
            .Select(Path.GetFullPath)
            .FirstOrDefault(File.Exists);

        if (appSettingsPath is null)
        {
            throw new InvalidOperationException(
                $"Could not find appsettings.json for ABCDMall.WebAPI. Current directory: {currentDirectory}");
        }

        var webApiBasePath = Path.GetDirectoryName(appSettingsPath)
            ?? throw new InvalidOperationException("Could not resolve WebAPI base path.");

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        IConfiguration configuration = new ConfigurationBuilder()
            //.SetBasePath(webApiBasePath) co tac dung la no se tim file appsettings.json trong thu muc webApiBasePath truoc,
            //neu khong tim thay thi no moi tim trong thu muc hien tai cua MoviesBookingDbContextFactory
            .SetBasePath(webApiBasePath)
            //  .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false) co tac dung la no se tim file appsettings.json trong thu muc webApiBasePath,
            //  neu khong tim thay thi no se nem loi va dung do,
            //  optional: false se chi ra rang file appsettings.json la bat buoc phai co,
            //  reloadOnChange: false se chi ra rang no se khong tu dong tai lai cau hinh neu file appsettings.json bi thay doi trong luc thiet ke
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            //  .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false) co tac dung la no se tim file appsettings.{environment}.json trong thu muc webApiBasePath,
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("ABCDMallConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'ABCDMallConnection' was not found.");

        var optionsBuilder = new DbContextOptionsBuilder<MoviesBookingDbContext>();

        optionsBuilder.UseSqlServer(
            connectionString,
            sql =>
            {
                sql.MigrationsAssembly(typeof(MoviesBookingDbContext).Assembly.FullName);
                sql.MigrationsHistoryTable(
                    "__EFMigrationsHistory_MoviesBooking",
                    MoviesBookingDbContext.DefaultSchema);
            });

        return new MoviesBookingDbContext(optionsBuilder.Options);
    }
}

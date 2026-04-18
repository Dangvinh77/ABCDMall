using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ABCDMall.Modules.UtilityMap.Infrastructure.Persistence.UtilityMap;

public class UtilityMapDbContextFactory : IDesignTimeDbContextFactory<UtilityMapDbContext>
{
    public UtilityMapDbContext CreateDbContext(string[] args)
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
            .SetBasePath(webApiBasePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("ABCDMallConnection")
            ?? throw new InvalidOperationException("Connection string 'ABCDMallConnection' was not found.");

        var optionsBuilder = new DbContextOptionsBuilder<UtilityMapDbContext>();
        optionsBuilder.UseSqlServer(
            connectionString,
            sql =>
            {
                sql.MigrationsAssembly(typeof(UtilityMapDbContext).Assembly.FullName);
                sql.MigrationsHistoryTable(
                    "__EFMigrationsHistory_UtilityMap",
                    UtilityMapDbContext.DefaultSchema);
            });

        return new UtilityMapDbContext(optionsBuilder.Options);
    }
}

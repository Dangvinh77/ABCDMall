using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ABCDMall.Modules.Shops.Infrastructure.Persistence.Shops;

public sealed class ShopsDbContextFactory : IDesignTimeDbContextFactory<ShopsDbContext>
{
    public ShopsDbContext CreateDbContext(string[] args)
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
            ?? configuration.GetConnectionString("ABCDMallShopsDBConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'ABCDMallConnection' or 'ABCDMallShopsDBConnection' was not found.");

        var optionsBuilder = new DbContextOptionsBuilder<ShopsDbContext>();
        optionsBuilder.UseSqlServer(
            connectionString,
            sql =>
            {
                sql.MigrationsAssembly(typeof(ShopsDbContext).Assembly.FullName);
                sql.MigrationsHistoryTable("__EFMigrationsHistory_Shops", ShopsDbContext.DefaultSchema);
            });

        return new ShopsDbContext(optionsBuilder.Options);
    }
}

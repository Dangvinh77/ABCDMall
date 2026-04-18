using ABCDMall.Modules.UtilityMap.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ABCDMall.Modules.UtilityMap.Infrastructure.Persistence.UtilityMap;

public class UtilityMapDbContext : DbContext
{
    public const string DefaultSchema = "utility_map";

    public UtilityMapDbContext(DbContextOptions<UtilityMapDbContext> options) : base(options)
    {
    }

    public DbSet<FloorPlan> FloorPlans => Set<FloorPlan>();
    public DbSet<MapLocation> MapLocations => Set<MapLocation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DefaultSchema);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        base.OnModelCreating(modelBuilder);
    }
}

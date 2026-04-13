using ABCDMall.Modules.FoodCourt.Domain.Entities;
using ABCDMall.Modules.UtilityMap.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Shared.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
     public DbSet<FoodItem> FoodItems { get; set; }   

    // --- Module FoodCourt ---
    public DbSet<FloorPlan> FloorPlans { get; set; }
    public DbSet<MapLocation> MapLocations { get; set; }

 protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Cấu hình quan hệ FloorPlan → MapLocation
    modelBuilder.Entity<MapLocation>()
        .HasOne(m => m.FloorPlan)
        .WithMany(f => f.Locations)
        .HasForeignKey(m => m.FloorPlanId)
        .OnDelete(DeleteBehavior.Cascade);
}
}
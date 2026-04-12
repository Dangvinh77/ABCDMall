using ABCDMall.Modules.FoodCourt.Domain.Entities;
using ABCDMall.Modules.UtilityMap.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Shared.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // --- Module FoodCourt ---
    public DbSet<FoodItem> FoodItems { get; set; }

    // --- Module UtilityMap ---
    public DbSet<FloorPlan> FloorPlans { get; set; }

    // --- Các module khác sẽ thêm vào đây khi làm ---
    // public DbSet<Movie> Movies { get; set; }
    // public DbSet<Shop> Shops { get; set; }
    // public DbSet<User> Users { get; set; }
    // public DbSet<Feedback> Feedbacks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
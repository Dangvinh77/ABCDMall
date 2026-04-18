using ABCDMall.Modules.FoodCourt.Domain.Entities;
using ABCDMall.Modules.FoodCourt.Infrastructure.Persistence.FoodCourt.Configurations;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.FoodCourt.Infrastructure.Persistence.FoodCourt;

public class FoodCourtDbContext : DbContext
{
    public const string DefaultSchema = "foodcourt";

    public FoodCourtDbContext(DbContextOptions<FoodCourtDbContext> options) : base(options)
    {
    }

    public DbSet<FoodItem> FoodItems => Set<FoodItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DefaultSchema);
        modelBuilder.ApplyConfiguration(new FoodItemConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}


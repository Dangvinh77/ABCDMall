using ABCDMall.Modules.Shops.Domain.Entities;
using ABCDMall.Modules.Shops.Infrastructure.Persistence.Shops.Configurations;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Shops.Infrastructure.Persistence.Shops;

public sealed class ShopsDbContext : DbContext
{
    public ShopsDbContext(DbContextOptions<ShopsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Shop> Shops => Set<Shop>();
    public DbSet<ShopProduct> ShopProducts => Set<ShopProduct>();
    public DbSet<ShopVoucher> ShopVouchers => Set<ShopVoucher>();
    public DbSet<ShopTag> ShopTags => Set<ShopTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ShopConfiguration());
        modelBuilder.ApplyConfiguration(new ShopProductConfiguration());
        modelBuilder.ApplyConfiguration(new ShopVoucherConfiguration());
        modelBuilder.ApplyConfiguration(new ShopTagConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}

using ABCDMall.Modules.Shops.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Shops.Infrastructure.Persistence.Shops.Configurations;

public sealed class ShopConfiguration : IEntityTypeConfiguration<Shop>
{
    public void Configure(EntityTypeBuilder<Shop> builder)
    {
        builder.ToTable("Shops");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(64);
        builder.Property(x => x.OwnerShopId).HasMaxLength(64);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Category).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Floor).HasMaxLength(80).IsRequired();
        builder.Property(x => x.LocationSlot).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Summary).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.LogoUrl).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.CoverImageUrl).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.OpenHours).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Badge).HasMaxLength(120);
        builder.Property(x => x.Offer).HasMaxLength(300);
        builder.HasIndex(x => x.OwnerShopId);
        builder.HasIndex(x => x.Slug).IsUnique();

        builder.HasMany(x => x.Tags).WithOne(x => x.Shop).HasForeignKey(x => x.ShopId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Products).WithOne(x => x.Shop).HasForeignKey(x => x.ShopId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Vouchers).WithOne(x => x.Shop).HasForeignKey(x => x.ShopId).OnDelete(DeleteBehavior.Cascade);
    }
}

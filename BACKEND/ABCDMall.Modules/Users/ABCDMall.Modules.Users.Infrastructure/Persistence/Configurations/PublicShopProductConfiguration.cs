using ABCDMall.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Users.Infrastructure.Persistence.Configurations;

internal sealed class PublicShopProductConfiguration : IEntityTypeConfiguration<PublicShopProduct>
{
    public void Configure(EntityTypeBuilder<PublicShopProduct> entity)
    {
        entity.ToTable("PublicShopProducts");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasMaxLength(64);
        entity.Property(x => x.ShopId).HasMaxLength(64).IsRequired();
        entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
        entity.Property(x => x.ImageUrl).HasMaxLength(1000).IsRequired();
        entity.Property(x => x.Price).HasPrecision(18, 2);
        entity.Property(x => x.OldPrice).HasPrecision(18, 2);

        entity.HasOne<PublicShop>()
            .WithMany()
            .HasForeignKey(x => x.ShopId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

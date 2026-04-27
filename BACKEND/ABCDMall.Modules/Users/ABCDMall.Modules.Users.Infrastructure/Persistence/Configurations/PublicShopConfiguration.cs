using ABCDMall.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Users.Infrastructure.Persistence.Configurations;

internal sealed class PublicShopConfiguration : IEntityTypeConfiguration<PublicShop>
{
    public void Configure(EntityTypeBuilder<PublicShop> entity)
    {
        entity.ToTable("Shops", "shops");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasMaxLength(64);
        entity.Property(x => x.OwnerShopId).HasMaxLength(64);
        entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
        entity.Property(x => x.Slug).HasMaxLength(200).IsRequired();
        entity.Property(x => x.Category).HasMaxLength(120).IsRequired();
        entity.Property(x => x.Floor).HasMaxLength(80).IsRequired();
        entity.Property(x => x.LocationSlot).HasMaxLength(80).IsRequired();
        entity.Property(x => x.Summary).HasMaxLength(1000).IsRequired();
        entity.Property(x => x.Description).HasMaxLength(4000).IsRequired();
        entity.Property(x => x.LogoUrl).HasMaxLength(1000).IsRequired();
        entity.Property(x => x.CoverImageUrl).HasMaxLength(1000).IsRequired();
        entity.Property(x => x.OpenHours).HasMaxLength(100).IsRequired();
        entity.Property(x => x.Badge).HasMaxLength(120);
        entity.Property(x => x.Offer).HasMaxLength(300);
        entity.Property(x => x.ShopStatus).HasMaxLength(20).IsRequired();
        entity.Property(x => x.OpeningDate);
    }
}

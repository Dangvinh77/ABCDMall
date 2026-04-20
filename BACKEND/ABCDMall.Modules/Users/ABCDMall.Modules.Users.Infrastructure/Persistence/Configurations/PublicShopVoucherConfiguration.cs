using ABCDMall.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Users.Infrastructure.Persistence.Configurations;

internal sealed class PublicShopVoucherConfiguration : IEntityTypeConfiguration<PublicShopVoucher>
{
    public void Configure(EntityTypeBuilder<PublicShopVoucher> entity)
    {
        entity.ToTable("ShopVouchers", "shops", table => table.ExcludeFromMigrations());
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasMaxLength(64);
        entity.Property(x => x.ShopId).HasMaxLength(64).IsRequired();
        entity.Property(x => x.Code).HasMaxLength(80).IsRequired();
        entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
        entity.Property(x => x.Description).HasMaxLength(500).IsRequired();
        entity.Property(x => x.ValidUntil).HasMaxLength(80).IsRequired();
    }
}

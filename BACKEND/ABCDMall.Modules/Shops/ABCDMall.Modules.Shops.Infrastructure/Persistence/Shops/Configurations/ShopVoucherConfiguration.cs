using ABCDMall.Modules.Shops.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Shops.Infrastructure.Persistence.Shops.Configurations;

public sealed class ShopVoucherConfiguration : IEntityTypeConfiguration<ShopVoucher>
{
    public void Configure(EntityTypeBuilder<ShopVoucher> builder)
    {
        builder.ToTable("ShopVouchers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(64);
        builder.Property(x => x.ShopId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.ValidUntil).HasMaxLength(120).IsRequired();
    }
}

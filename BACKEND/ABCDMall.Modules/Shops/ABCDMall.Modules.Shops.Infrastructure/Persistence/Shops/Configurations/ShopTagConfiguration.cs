using ABCDMall.Modules.Shops.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Shops.Infrastructure.Persistence.Shops.Configurations;

public sealed class ShopTagConfiguration : IEntityTypeConfiguration<ShopTag>
{
    public void Configure(EntityTypeBuilder<ShopTag> builder)
    {
        builder.ToTable("ShopTags");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(64);
        builder.Property(x => x.ShopId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(120).IsRequired();
    }
}

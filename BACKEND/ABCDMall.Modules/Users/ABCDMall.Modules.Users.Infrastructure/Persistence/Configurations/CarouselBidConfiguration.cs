using ABCDMall.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Users.Infrastructure.Persistence.Configurations;

internal sealed class CarouselBidConfiguration : IEntityTypeConfiguration<CarouselBid>
{
    public void Configure(EntityTypeBuilder<CarouselBid> entity)
    {
        entity.HasKey(x => x.Id);

        entity.HasIndex(x => x.ShopId);
        entity.HasIndex(x => x.TargetMondayDate);
        entity.HasIndex(x => x.Status);

        entity.Property(x => x.Id)
            .HasMaxLength(64)
            .HasDefaultValueSql(ModelConfigurationDefaults.SqlServerStringIdDefault);

        entity.Property(x => x.ShopId)
            .HasMaxLength(64)
            .IsRequired();

        entity.Property(x => x.BidAmount)
            .HasPrecision(18, 2);

        entity.Property(x => x.TemplateData)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        entity.Property(x => x.TemplateType)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        entity.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
    }
}

using ABCDMall.Modules.UtilityMap.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.UtilityMap.Infrastructure.Persistence.UtilityMap.Configurations;

public class MapLocationConfiguration : IEntityTypeConfiguration<MapLocation>
{
    public void Configure(EntityTypeBuilder<MapLocation> builder)
    {
        builder.ToTable("MapLocations");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.ShopName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.LocationSlot)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(l => l.ShopUrl)
            .HasMaxLength(500);

        builder.Property(l => l.StorefrontImageUrl)
            .HasMaxLength(500);

        builder.Property(l => l.X)
            .HasColumnType("float");

        builder.Property(l => l.Y)
            .HasColumnType("float");

        builder.Property(l => l.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("Available");

        builder.Property(l => l.ShopInfoId)
            .HasMaxLength(64);

        builder.HasIndex(l => l.ShopInfoId);
        builder.HasIndex(l => l.Status);
    }
}
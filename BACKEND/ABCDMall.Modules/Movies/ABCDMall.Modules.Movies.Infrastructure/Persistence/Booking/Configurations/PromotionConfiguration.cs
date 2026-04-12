using ABCDMall.Modules.Movies.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking.Configurations;

public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.ToTable("Promotions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.PercentageValue).HasColumnType("decimal(5,2)");
        builder.Property(x => x.FlatDiscountValue).HasColumnType("decimal(18,2)");
        builder.Property(x => x.MaximumDiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.MinimumSpendAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.MetadataJson).HasColumnType("nvarchar(max)");

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => new { x.Status, x.ValidFromUtc, x.ValidToUtc });

        builder.HasMany(x => x.Rules)
            .WithOne(x => x.Promotion)
            .HasForeignKey(x => x.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Redemptions)
            .WithOne(x => x.Promotion)
            .HasForeignKey(x => x.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

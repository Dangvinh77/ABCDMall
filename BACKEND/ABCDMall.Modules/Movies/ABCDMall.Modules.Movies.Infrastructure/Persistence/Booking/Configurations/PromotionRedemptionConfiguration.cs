using ABCDMall.Modules.Movies.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking.Configurations;

public class PromotionRedemptionConfiguration : IEntityTypeConfiguration<PromotionRedemption>
{
    public void Configure(EntityTypeBuilder<PromotionRedemption> builder)
    {
        builder.ToTable("PromotionRedemptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CouponCode)
            .HasMaxLength(50);

        builder.Property(x => x.DiscountAmount)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(x => x.PromotionId);
        builder.HasIndex(x => x.BookingId);
        builder.HasIndex(x => x.GuestCustomerId);
        builder.HasIndex(x => x.CouponCode);
    }
}

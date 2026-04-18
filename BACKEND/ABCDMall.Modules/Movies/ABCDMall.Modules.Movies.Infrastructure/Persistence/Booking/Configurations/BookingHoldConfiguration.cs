using ABCDMall.Modules.Movies.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking.Configurations
{
    public class BookingHoldConfiguration:IEntityTypeConfiguration<BookingHold>
    {
        public void Configure(EntityTypeBuilder<BookingHold> builder)
        {
            builder.ToTable("BookingHolds");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.HoldCode)
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(x => x.SessionId)
                .HasMaxLength(100);

            builder.Property(x => x.SeatSubtotal).HasColumnType("decimal(18,2)");
            builder.Property(x => x.ComboSubtotal).HasColumnType("decimal(18,2)");
            builder.Property(x => x.ServiceFee).HasColumnType("decimal(18,2)");
            builder.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
            builder.Property(x => x.GrandTotal).HasColumnType("decimal(18,2)");
            builder.Property(x => x.PromotionSnapshotJson).HasColumnType("nvarchar(max)");
            builder.Property(x => x.ComboSnapshotJson).HasColumnType("nvarchar(max)");

            builder.Property(x => x.CreatedAtUtc).IsRequired();
            builder.Property(x => x.UpdatedAtUtc).IsRequired();
            builder.Property(x => x.ExpiresAtUtc).IsRequired();

            builder.HasIndex(x => x.HoldCode).IsUnique();
            builder.HasIndex(x => new { x.ShowtimeId, x.Status, x.ExpiresAtUtc });
            builder.HasIndex(x => x.PromotionId);

            builder.HasMany(x => x.Seats)
                .WithOne(x => x.BookingHold)
                .HasForeignKey(x => x.BookingHoldId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

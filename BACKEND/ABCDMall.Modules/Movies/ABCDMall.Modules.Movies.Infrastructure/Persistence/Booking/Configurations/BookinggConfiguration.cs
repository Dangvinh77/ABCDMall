using ABCDMall.Modules.Movies.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking.Configurations;

public class BookinggConfiguration : IEntityTypeConfiguration<Bookingg>
{
    public void Configure(EntityTypeBuilder<Bookingg> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BookingCode)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.CustomerName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.CustomerEmail)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.CustomerPhoneNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.PromotionSnapshotJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.SeatSubtotal).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ComboSubtotal).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ServiceFee).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.GrandTotal).HasColumnType("decimal(18,2)");

        builder.HasIndex(x => x.BookingCode).IsUnique();//Đảm bảo mã đặt chỗ là duy nhất
        builder.HasIndex(x => x.ShowtimeId);
        builder.HasIndex(x => x.GuestCustomerId);
        builder.HasIndex(x => x.BookingHoldId)
            .IsUnique()
            .HasFilter("[BookingHoldId] IS NOT NULL");

        builder.HasOne(x => x.GuestCustomer)
            .WithMany(x => x.Bookings)
            .HasForeignKey(x => x.GuestCustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.Booking) // Quan hệ một-nhiều với BookingItem, mỗi BookingItem có một Bookingg
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Payments)
            .WithOne(x => x.Booking)
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Tickets)
            .WithOne(x => x.Booking)
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

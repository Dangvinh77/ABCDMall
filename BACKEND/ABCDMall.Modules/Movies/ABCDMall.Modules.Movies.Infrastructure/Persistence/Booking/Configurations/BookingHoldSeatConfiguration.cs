using ABCDMall.Modules.Movies.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking.Configurations;

public class BookingHoldSeatConfiguration : IEntityTypeConfiguration<BookingHoldSeat>
{
    public void Configure(EntityTypeBuilder<BookingHoldSeat> builder)
    {
        builder.ToTable("BookingHoldSeats");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SeatCode)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.SeatType)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.CoupleGroupCode)
            .HasMaxLength(50);

        builder.Property(x => x.UnitPrice)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(x => new { x.BookingHoldId, x.SeatInventoryId }).IsUnique();// Đảm bảo rằng mỗi ghế trong một BookingHold là duy nhất
        builder.HasIndex(x => x.SeatInventoryId);
    }
}

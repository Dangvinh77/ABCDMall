using ABCDMall.Modules.Movies.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking.Configurations;

public class BookingItemConfiguration : IEntityTypeConfiguration<BookingItem>
{
    public void Configure(EntityTypeBuilder<BookingItem> builder)
    {
        builder.ToTable("BookingItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ItemType)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.ItemCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.LineTotal).HasColumnType("decimal(18,2)");

        builder.HasIndex(x => x.BookingId);
        builder.HasIndex(x => x.SeatInventoryId);
    }
}

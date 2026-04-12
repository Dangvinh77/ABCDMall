using ABCDMall.Modules.Movies.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TicketCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.SeatCode)
            .HasMaxLength(20);

        builder.Property(x => x.DeliveryStatus)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.QrCodeContent)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(x => x.TicketCode).IsUnique();
        builder.HasIndex(x => x.BookingId);
        builder.HasIndex(x => x.SeatInventoryId);
    }
}

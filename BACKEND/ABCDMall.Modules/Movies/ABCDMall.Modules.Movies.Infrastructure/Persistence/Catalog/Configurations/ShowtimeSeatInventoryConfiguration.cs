using ABCDMall.Modules.Movies.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Catalog.Configurations;

public class ShowtimeSeatInventoryConfiguration : IEntityTypeConfiguration<ShowtimeSeatInventory>
{
    public void Configure(EntityTypeBuilder<ShowtimeSeatInventory> builder)
    {
        builder.ToTable("ShowtimeSeatInventory");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SeatCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.RowLabel).HasMaxLength(10).IsRequired();
        builder.Property(x => x.CoupleGroupCode).HasMaxLength(50);
        builder.Property(x => x.Price).HasColumnType("decimal(18,2)");

        builder.HasIndex(x => new { x.ShowtimeId, x.SeatCode }).IsUnique();
        builder.HasIndex(x => new { x.ShowtimeId, x.Status });

        builder.HasOne(x => x.Showtime)
            .WithMany(x => x.SeatInventories)
            .HasForeignKey(x => x.ShowtimeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.HallSeat)
            .WithMany(x => x.ShowtimeSeatInventories)
            .HasForeignKey(x => x.HallSeatId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

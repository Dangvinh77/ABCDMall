using ABCDMall.Modules.Movies.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Catalog.Configurations;

public class HallSeatConfiguration : IEntityTypeConfiguration<HallSeat>
{
    public void Configure(EntityTypeBuilder<HallSeat> builder)
    {
        builder.ToTable("HallSeats");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SeatCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.RowLabel).HasMaxLength(10).IsRequired();
        builder.Property(x => x.CoupleGroupCode).HasMaxLength(50);

        builder.HasIndex(x => new { x.HallId, x.SeatCode }).IsUnique();
        builder.HasIndex(x => new { x.HallId, x.RowLabel, x.ColumnNumber }).IsUnique();

        builder.HasOne(x => x.Hall)
            .WithMany(x => x.HallSeats)
            .HasForeignKey(x => x.HallId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

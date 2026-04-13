using ABCDMall.Modules.Movies.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Catalog.Configurations;

public class ShowtimeConfiguration : IEntityTypeConfiguration<Showtime>
{
    public void Configure(EntityTypeBuilder<Showtime> builder)
    {
        builder.ToTable("Showtimes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BasePrice).HasColumnType("decimal(18,2)");

        builder.HasIndex(x => new { x.HallId, x.StartAtUtc });
        builder.HasIndex(x => new { x.MovieId, x.BusinessDate, x.Status });
        builder.HasIndex(x => new { x.CinemaId, x.BusinessDate, x.Status });

        builder.HasOne(x => x.Movie)
            .WithMany(x => x.Showtimes)
            .HasForeignKey(x => x.MovieId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Cinema)
            .WithMany(x => x.Showtimes)
            .HasForeignKey(x => x.CinemaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Hall)
            .WithMany(x => x.Showtimes)
            .HasForeignKey(x => x.HallId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

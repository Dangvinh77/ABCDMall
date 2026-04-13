using ABCDMall.Modules.Movies.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Catalog.Configurations;

public class HallConfiguration : IEntityTypeConfiguration<Hall>
{
    public void Configure(EntityTypeBuilder<Hall> builder)
    {
        builder.ToTable("Halls");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.HallCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();

        builder.HasIndex(x => new { x.CinemaId, x.HallCode }).IsUnique();

        builder.HasOne(x => x.Cinema)
            .WithMany(x => x.Halls)
            .HasForeignKey(x => x.CinemaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

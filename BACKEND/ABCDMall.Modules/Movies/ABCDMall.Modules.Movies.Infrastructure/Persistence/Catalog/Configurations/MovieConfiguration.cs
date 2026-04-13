using ABCDMall.Modules.Movies.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Catalog.Configurations;

public class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.ToTable("Movies");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Synopsis).HasMaxLength(4000);
        builder.Property(x => x.PosterUrl).HasMaxLength(1000);
        builder.Property(x => x.TrailerUrl).HasMaxLength(1000);
        builder.Property(x => x.RatingLabel).HasMaxLength(20);

        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => new { x.Status, x.ReleaseDate });
    }
}

using ABCDMall.Modules.Movies.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Catalog.Configurations;

public class MovieGenreConfiguration : IEntityTypeConfiguration<MovieGenre>
{
    public void Configure(EntityTypeBuilder<MovieGenre> builder)
    {
        builder.ToTable("MovieGenres");

        builder.HasKey(x => new { x.MovieId, x.GenreId });

        builder.HasOne(x => x.Movie)
            .WithMany(x => x.MovieGenres)
            .HasForeignKey(x => x.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Genre)
            .WithMany(x => x.MovieGenres)
            .HasForeignKey(x => x.GenreId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

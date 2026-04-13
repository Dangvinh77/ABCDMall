using ABCDMall.Modules.Movies.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Catalog.Configurations;

public class MovieCreditConfiguration : IEntityTypeConfiguration<MovieCredit>
{
    public void Configure(EntityTypeBuilder<MovieCredit> builder)
    {
        builder.ToTable("MovieCredits");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreditType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.RoleName).HasMaxLength(150).IsRequired();

        builder.HasIndex(x => new { x.MovieId, x.PersonId, x.CreditType });

        builder.HasOne(x => x.Movie)
            .WithMany(x => x.Credits)
            .HasForeignKey(x => x.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Person)
            .WithMany(x => x.MovieCredits)
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

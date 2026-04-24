using ABCDMall.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Users.Infrastructure.Persistence.Configurations;

internal sealed class MovieCarouselAdConfiguration : IEntityTypeConfiguration<MovieCarouselAd>
{
    public void Configure(EntityTypeBuilder<MovieCarouselAd> entity)
    {
        entity.HasKey(x => x.Id);

        entity.HasIndex(x => x.TargetMondayDate)
            .IsUnique();

        entity.HasIndex(x => x.IsActive);

        entity.Property(x => x.Id)
            .HasMaxLength(64)
            .HasDefaultValueSql(ModelConfigurationDefaults.SqlServerStringIdDefault);

        entity.Property(x => x.ImageUrl)
            .HasMaxLength(500)
            .IsRequired();

        entity.Property(x => x.Description)
            .HasMaxLength(1000)
            .IsRequired();
    }
}

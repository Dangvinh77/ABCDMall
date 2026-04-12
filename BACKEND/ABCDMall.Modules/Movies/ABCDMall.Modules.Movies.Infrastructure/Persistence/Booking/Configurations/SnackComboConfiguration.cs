using ABCDMall.Modules.Movies.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking.Configurations;

public class SnackComboConfiguration : IEntityTypeConfiguration<SnackCombo>
{
    public void Configure(EntityTypeBuilder<SnackCombo> builder)
    {
        builder.ToTable("SnackCombos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(1000);

        builder.Property(x => x.Price)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.IsActive);
    }
}

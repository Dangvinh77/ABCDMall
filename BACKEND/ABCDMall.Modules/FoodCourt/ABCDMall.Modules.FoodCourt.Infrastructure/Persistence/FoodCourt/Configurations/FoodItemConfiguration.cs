using ABCDMall.Modules.FoodCourt.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.FoodCourt.Infrastructure.Persistence.FoodCourt.Configurations;

public class FoodItemConfiguration : IEntityTypeConfiguration<FoodItem>
{
    public void Configure(EntityTypeBuilder<FoodItem> builder)
    {
        builder.ToTable("FoodItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasMaxLength(64);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ImageUrl).HasMaxLength(1000);
        builder.Property(x => x.Slug).HasMaxLength(200).IsRequired();
        builder.Property(x => x.MallSlug).HasMaxLength(200);
        builder.Property(x => x.CategorySlug).HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(4000);

        builder.HasIndex(x => x.Slug).IsUnique();
    }
}


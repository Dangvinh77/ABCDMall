using ABCDMall.Modules.FoodCourt.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.FoodCourt.Infrastructure.Persistence.FoodCourt.Configurations;

public class FoodMenuItemConfiguration : IEntityTypeConfiguration<FoodMenuItem>
{
    public void Configure(EntityTypeBuilder<FoodMenuItem> builder)
    {
        builder.ToTable("FoodMenuItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasMaxLength(64);
        builder.Property(x => x.FoodStallId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Price).HasPrecision(18, 2);
        builder.Property(x => x.Note).HasMaxLength(2000);
        builder.Property(x => x.Tag).HasMaxLength(100);
        builder.Property(x => x.ImageUrl).HasMaxLength(1000);
        builder.Property(x => x.IngredientsJson).HasMaxLength(4000);

        builder.HasIndex(x => new { x.FoodStallId, x.DisplayOrder });
    }
}

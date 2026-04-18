using ABCDMall.Modules.UtilityMap.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.UtilityMap.Infrastructure.Persistence.UtilityMap.Configurations;

public class FloorPlanConfiguration : IEntityTypeConfiguration<FloorPlan>
{
    public void Configure(EntityTypeBuilder<FloorPlan> builder)
    {
        builder.ToTable("FloorPlans");
        
        builder.HasKey(f => f.Id);
        
        builder.Property(f => f.FloorLevel)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(f => f.Description)
            .HasMaxLength(500);

        builder.Property(f => f.BlueprintImageUrl)
            .HasMaxLength(500);

        builder.HasMany(f => f.Locations)
            .WithOne(l => l.FloorPlan)
            .HasForeignKey(l => l.FloorPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

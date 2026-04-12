using ABCDMall.Modules.Movies.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking.Configurations;

public class PromotionRuleConfiguration : IEntityTypeConfiguration<PromotionRule>
{
    public void Configure(EntityTypeBuilder<PromotionRule> builder)
    {
        builder.ToTable("PromotionRules");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RuleValue)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ThresholdValue)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(x => x.PromotionId);
        builder.HasIndex(x => new { x.PromotionId, x.SortOrder });
    }
}

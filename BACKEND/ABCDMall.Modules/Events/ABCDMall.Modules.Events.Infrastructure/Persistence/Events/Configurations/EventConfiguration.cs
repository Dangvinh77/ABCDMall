using ABCDMall.Modules.Events.Domain.Entities;
using ABCDMall.Modules.Events.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Events.Infrastructure.Persistence.Events.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();
        builder.Property(x => x.Title)
            .HasMaxLength(250)
            .IsRequired();
        builder.Property(x => x.Description)
            .HasMaxLength(4000);
        builder.Property(x => x.ImageUrl)
            .HasMaxLength(1000);
        builder.Property(x => x.LocationType)
            .HasConversion<int>()
            .IsRequired();
        builder.Property(x => x.ShopId)
            .HasMaxLength(64);
        builder.Property(x => x.ApprovalStatus)
            .HasConversion<int>()
            .HasDefaultValue(EventApprovalStatus.Pending)
            .IsRequired();
        builder.Property(x => x.HasGiftRegistration)
            .HasDefaultValue(false)
            .IsRequired();
        builder.Property(x => x.GiftDescription)
            .HasMaxLength(500);
        builder.Property(x => x.CreatedAt)
            .IsRequired();
        builder.HasIndex(x => new { x.LocationType, x.StartDateTime, x.EndDateTime });
        builder.HasIndex(x => new { x.ApprovalStatus, x.StartDateTime, x.EndDateTime });
        builder.HasMany(x => x.Registrations)
            .WithOne(x => x.Event)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

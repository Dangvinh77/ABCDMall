using ABCDMall.Modules.Events.Domain.Entities;
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
            .ValueGeneratedNever(); // Guid được set từ application layer

        builder.Property(x => x.Title)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(4000);

        builder.Property(x => x.CoverImageUrl)
            .HasMaxLength(1000);

        builder.Property(x => x.Location)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.EventType)
            .IsRequired()
            .HasConversion<int>(); // lưu DB dạng int (1 / 2)

        builder.Property(x => x.ShopId)
            .HasMaxLength(64);

        builder.Property(x => x.ShopName)
            .HasMaxLength(300);

        builder.Property(x => x.IsHot)
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Index để query nhanh theo IsHot (Banner Slider)
        builder.HasIndex(x => x.IsHot);

        // Index để query theo EventType
        builder.HasIndex(x => x.EventType);

        // Index theo thời gian (lọc Upcoming / Ongoing / Ended)
        builder.HasIndex(x => new { x.StartDate, x.EndDate });
    }
}

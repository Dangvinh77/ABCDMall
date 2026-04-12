using ABCDMall.Modules.Movies.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking.Configurations;

public class OutboxEventConfiguration : IEntityTypeConfiguration<OutboxEvent>
{
    public void Configure(EntityTypeBuilder<OutboxEvent> builder)
    {
        builder.ToTable("OutboxEvents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.PayloadJson)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.LastError)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(x => new { x.Status, x.OccurredAtUtc });
    }
}

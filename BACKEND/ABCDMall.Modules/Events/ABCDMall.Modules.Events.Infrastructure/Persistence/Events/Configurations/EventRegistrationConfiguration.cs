using ABCDMall.Modules.Events.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Events.Infrastructure.Persistence.Events.Configurations;

public sealed class EventRegistrationConfiguration : IEntityTypeConfiguration<EventRegistration>
{
    public void Configure(EntityTypeBuilder<EventRegistration> builder)
    {
        builder.ToTable("EventRegistrations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CustomerName)
            .HasMaxLength(120)
            .IsRequired();
        builder.Property(x => x.CustomerEmail)
            .HasMaxLength(160)
            .IsRequired();
        builder.Property(x => x.CustomerPhone)
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(x => x.RedeemCode)
            .HasMaxLength(6)
            .IsRequired();
        builder.Property(x => x.RegisteredAt)
            .IsRequired();

        builder.HasIndex(x => new { x.EventId, x.CustomerEmail });
        builder.HasIndex(x => x.RedeemCode);
    }
}

using ABCDMall.Modules.Movies.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EntityName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.EntityId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Action)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ActorId)
            .HasMaxLength(100);

        builder.Property(x => x.ChangesJson)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(x => new { x.EntityName, x.EntityId, x.CreatedAtUtc });
    }
}

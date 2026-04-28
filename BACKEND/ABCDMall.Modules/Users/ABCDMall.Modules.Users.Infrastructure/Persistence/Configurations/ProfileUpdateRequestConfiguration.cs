using ABCDMall.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Users.Infrastructure.Persistence.Configurations;

internal sealed class ProfileUpdateRequestConfiguration : IEntityTypeConfiguration<ProfileUpdateRequest>
{
    public void Configure(EntityTypeBuilder<ProfileUpdateRequest> entity)
    {
        entity.HasKey(x => x.Id);
        entity.HasIndex(x => x.Status);
        entity.HasIndex(x => new { x.UserId, x.Status });

        entity.Property(x => x.Id).HasMaxLength(64).HasDefaultValueSql(ModelConfigurationDefaults.SqlServerStringIdDefault);
        entity.Property(x => x.UserId).HasMaxLength(64).IsRequired();
        entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
        entity.Property(x => x.CurrentFullName).HasMaxLength(200);
        entity.Property(x => x.CurrentAddress).HasMaxLength(500);
        entity.Property(x => x.CurrentCCCD).HasMaxLength(50);
        entity.Property(x => x.CurrentCccdFrontImage).HasMaxLength(500);
        entity.Property(x => x.CurrentCccdBackImage).HasMaxLength(500);
        entity.Property(x => x.RequestedFullName).HasMaxLength(200);
        entity.Property(x => x.RequestedAddress).HasMaxLength(500);
        entity.Property(x => x.RequestedCCCD).HasMaxLength(50);
        entity.Property(x => x.RequestedCccdFrontImage).HasMaxLength(500);
        entity.Property(x => x.RequestedCccdBackImage).HasMaxLength(500);
        entity.Property(x => x.Status).HasMaxLength(30).HasDefaultValue("Pending").IsRequired();
        entity.Property(x => x.ReviewedByAdminId).HasMaxLength(64);
        entity.Property(x => x.ReviewNote).HasMaxLength(500);
    }
}

using ABCDMall.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Users.Infrastructure.Persistence.Configurations;

internal sealed class ProfileUpdateHistoryConfiguration : IEntityTypeConfiguration<ProfileUpdateHistory>
{
    public void Configure(EntityTypeBuilder<ProfileUpdateHistory> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasMaxLength(64).HasDefaultValueSql(ModelConfigurationDefaults.SqlServerStringIdDefault);
        entity.Property(x => x.UserId).HasMaxLength(64).IsRequired();
        entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
        entity.Property(x => x.PreviousFullName).HasMaxLength(200);
        entity.Property(x => x.PreviousAddress).HasMaxLength(500);
        entity.Property(x => x.PreviousImage).HasMaxLength(500);
        entity.Property(x => x.PreviousCCCD).HasMaxLength(50);
        entity.Property(x => x.PreviousCccdFrontImage).HasMaxLength(500);
        entity.Property(x => x.PreviousCccdBackImage).HasMaxLength(500);
        entity.Property(x => x.UpdatedFullName).HasMaxLength(200);
        entity.Property(x => x.UpdatedAddress).HasMaxLength(500);
        entity.Property(x => x.UpdatedImage).HasMaxLength(500);
        entity.Property(x => x.UpdatedCCCD).HasMaxLength(50);
        entity.Property(x => x.UpdatedCccdFrontImage).HasMaxLength(500);
        entity.Property(x => x.UpdatedCccdBackImage).HasMaxLength(500);
    }
}

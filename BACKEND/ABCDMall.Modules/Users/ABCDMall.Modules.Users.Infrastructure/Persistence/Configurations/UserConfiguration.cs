using ABCDMall.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Users.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.HasKey(x => x.Id);
        entity.HasIndex(x => x.Email).IsUnique();
        entity.HasIndex(x => x.CCCD).IsUnique().HasFilter("[CCCD] IS NOT NULL");

        entity.Property(x => x.Id).HasMaxLength(64).HasDefaultValueSql(ModelConfigurationDefaults.SqlServerStringIdDefault);
        entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
        entity.Property(x => x.Password).IsRequired();
        entity.Property(x => x.Role).HasMaxLength(50).IsRequired();
        entity.Property(x => x.FullName).HasMaxLength(200);
        entity.Property(x => x.ShopId).HasMaxLength(64);
        entity.Property(x => x.Image).HasMaxLength(500);
        entity.Property(x => x.Address).HasMaxLength(500);
        entity.Property(x => x.CCCD).HasMaxLength(50);
        entity.Property(x => x.CccdFrontImage).HasMaxLength(500);
        entity.Property(x => x.CccdBackImage).HasMaxLength(500);
        entity.Property(x => x.IsActive).HasDefaultValue(true);
        entity.Property(x => x.LoginOtpCode).HasMaxLength(10);
        entity.Property(x => x.MustChangePassword).HasDefaultValue(false);
        entity.Property(x => x.OneTimePasswordHash).HasMaxLength(500);
        entity.Property(x => x.PasswordSetupToken).HasMaxLength(128);
    }
}

using ABCDMall.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Users.Infrastructure.Persistence.Configurations;

internal sealed class ForgotPasswordOtpConfiguration : IEntityTypeConfiguration<ForgotPasswordOtp>
{
    public void Configure(EntityTypeBuilder<ForgotPasswordOtp> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasMaxLength(64).HasDefaultValueSql(ModelConfigurationDefaults.SqlServerStringIdDefault);
        entity.Property(x => x.UserId).HasMaxLength(64).IsRequired();
        entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
        entity.Property(x => x.Otp).HasMaxLength(10).IsRequired();
        entity.Property(x => x.NewPasswordHash).IsRequired();
    }
}

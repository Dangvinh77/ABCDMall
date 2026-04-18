using ABCDMall.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Users.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> entity)
    {
        entity.HasKey(x => x.Id);
        entity.HasIndex(x => x.Token).IsUnique();

        entity.Property(x => x.Id).HasMaxLength(64).HasDefaultValueSql(ModelConfigurationDefaults.SqlServerStringIdDefault);
        entity.Property(x => x.UserId).HasMaxLength(64).IsRequired();
        entity.Property(x => x.Token).HasMaxLength(500).IsRequired();
    }
}

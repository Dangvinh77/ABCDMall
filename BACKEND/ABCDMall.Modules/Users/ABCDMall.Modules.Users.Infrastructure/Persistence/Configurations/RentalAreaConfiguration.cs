using ABCDMall.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Users.Infrastructure.Persistence.Configurations;

internal sealed class RentalAreaConfiguration : IEntityTypeConfiguration<RentalArea>
{
    public void Configure(EntityTypeBuilder<RentalArea> entity)
    {
        entity.HasKey(x => x.Id);
        entity.HasIndex(x => x.AreaCode).IsUnique();

        entity.Property(x => x.Id).HasMaxLength(64).HasDefaultValueSql(ModelConfigurationDefaults.SqlServerStringIdDefault);
        entity.Property(x => x.AreaCode).HasMaxLength(50).IsRequired();
        entity.Property(x => x.Floor).HasMaxLength(20).IsRequired();
        entity.Property(x => x.AreaName).HasMaxLength(200).IsRequired();
        entity.Property(x => x.Size).HasMaxLength(50).IsRequired();
        entity.Property(x => x.MonthlyRent).HasPrecision(18, 2);
        entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
        entity.Property(x => x.TenantName).HasMaxLength(200);
        entity.Property(x => x.ShopInfoId).HasMaxLength(64);
        entity.HasIndex(x => x.ShopInfoId);
    }
}

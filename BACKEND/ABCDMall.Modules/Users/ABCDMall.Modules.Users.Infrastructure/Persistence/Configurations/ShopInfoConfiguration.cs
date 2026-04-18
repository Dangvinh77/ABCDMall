using ABCDMall.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Users.Infrastructure.Persistence.Configurations;

internal sealed class ShopInfoConfiguration : IEntityTypeConfiguration<ShopInfo>
{
    public void Configure(EntityTypeBuilder<ShopInfo> entity)
    {
        entity.HasKey(x => x.Id);
        entity.HasIndex(x => x.CCCD).IsUnique().HasFilter("[CCCD] IS NOT NULL");

        entity.Property(x => x.Id).HasMaxLength(64).HasDefaultValueSql(ModelConfigurationDefaults.SqlServerStringIdDefault);
        entity.Property(x => x.ShopName).HasMaxLength(200).IsRequired();
        entity.Property(x => x.ManagerName).HasMaxLength(200);
        entity.Property(x => x.CCCD).HasMaxLength(50);
        entity.Property(x => x.RentalLocation).HasMaxLength(100).IsRequired();
        entity.Property(x => x.Month).HasMaxLength(50).IsRequired();
        entity.Property(x => x.ElectricityUsage).HasMaxLength(100).IsRequired();
        entity.Property(x => x.ElectricityFee).HasPrecision(18, 2);
        entity.Property(x => x.WaterUsage).HasMaxLength(100).IsRequired();
        entity.Property(x => x.WaterFee).HasPrecision(18, 2);
        entity.Property(x => x.ServiceFee).HasPrecision(18, 2);
        entity.Property(x => x.TotalDue).HasPrecision(18, 2);
        entity.Property(x => x.ContractImage).HasMaxLength(500);
        entity.Property(x => x.ContractImages).HasMaxLength(500);
    }
}

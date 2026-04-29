using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Application.DTOs;
using ABCDMall.Modules.Users.Application.Services;
using ABCDMall.Modules.Users.Application.Services.RentalAreas;
using ABCDMall.Modules.Users.Domain.Entities;
using ABCDMall.Modules.Users.Infrastructure;
using ABCDMall.Modules.UtilityMap.Domain.Entities;
using ABCDMall.Modules.UtilityMap.Infrastructure.Persistence.UtilityMap;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ABCDMall.Modules.Users.Tests;

public class RentalAreaRegistrationIntegrationTests
{
    [Fact]
    public async Task RegisterTenantAsync_registers_available_map_slot_even_without_legacy_rental_area_row()
    {
        await using var mallContext = CreateMallContext();
        await using var utilityMapContext = CreateUtilityMapContext();

        mallContext.Users.Add(new User
        {
            Id = "users-manager-031",
            Email = "manager31@abcdmall.local",
            Role = "Manager",
            FullName = "Prospect Manager 31",
            CCCD = "089204000031",
            IsActive = true
        });
        utilityMapContext.FloorPlans.Add(new FloorPlan
        {
            Id = 1,
            FloorLevel = "Tang 1",
            Description = "Test floor",
            BlueprintImageUrl = "/maps/test.png",
        });
        utilityMapContext.MapLocations.Add(new MapLocation
        {
            Id = 22,
            FloorPlanId = 1,
            ShopName = "Placeholder",
            LocationSlot = "1-09",
            ShopUrl = "/shops/pedro",
            X = 1,
            Y = 1,
            StorefrontImageUrl = "/img/test.png",
            Status = "Available"
        });
        await mallContext.SaveChangesAsync();
        await utilityMapContext.SaveChangesAsync();

        var service = new RentalAreaCommandService(
            null!,
            new RentalAreaCommandRepository(mallContext, utilityMapContext),
            new FakeFileStorageService());

        var result = await service.RegisterTenantAsync("22", new RegisterTenantDto
        {
            CCCD = "089204000031",
            Location = "1-09",
            BusinessType = "Shop",
            StartDate = DateTime.Today.AddDays(1),
            ElectricityFee = 3500m,
            WaterFee = 15000m,
            ServiceFee = 600000m,
            LeaseTermDays = 180,
            ContractImage = new FormFile(Stream.Null, 0, 0, "contract", "contract.png")
        });

        Assert.Equal(ApplicationResultStatus.Ok, result.Status);
    }

    [Fact]
    public async Task RegisterTenantAsync_updates_existing_legacy_rental_area_matching_map_slot_without_duplicate_insert()
    {
        await using var mallContext = CreateMallContext();
        await using var utilityMapContext = CreateUtilityMapContext();

        mallContext.Users.Add(new User
        {
            Id = "users-manager-032",
            Email = "manager32@abcdmall.local",
            Role = "Manager",
            FullName = "Prospect Manager 32",
            CCCD = "089204000032",
            IsActive = true
        });
        mallContext.RentalAreas.Add(new RentalArea
        {
            Id = "legacy-rental-1",
            AreaCode = "1-09",
            Floor = "Tang 1",
            AreaName = "Legacy Slot 1-09",
            Status = "Available"
        });
        utilityMapContext.FloorPlans.Add(new FloorPlan
        {
            Id = 1,
            FloorLevel = "Tang 1",
            Description = "Test floor",
            BlueprintImageUrl = "/maps/test.png",
        });
        utilityMapContext.MapLocations.Add(new MapLocation
        {
            Id = 22,
            FloorPlanId = 1,
            ShopName = "Placeholder",
            LocationSlot = "1-09",
            ShopUrl = "/shops/pedro",
            X = 1,
            Y = 1,
            StorefrontImageUrl = "/img/test.png",
            Status = "Available"
        });
        await mallContext.SaveChangesAsync();
        await utilityMapContext.SaveChangesAsync();

        var service = new RentalAreaCommandService(
            null!,
            new RentalAreaCommandRepository(mallContext, utilityMapContext),
            new FakeFileStorageService());

        var result = await service.RegisterTenantAsync("22", new RegisterTenantDto
        {
            CCCD = "089204000032",
            Location = "1-09",
            BusinessType = "Shop",
            StartDate = DateTime.Today.AddDays(1),
            ElectricityFee = 3500m,
            WaterFee = 15000m,
            ServiceFee = 600000m,
            LeaseTermDays = 180,
            ContractImage = new FormFile(Stream.Null, 0, 0, "contract", "contract.png")
        });

        Assert.Equal(ApplicationResultStatus.Ok, result.Status);

        var rentalAreas = await mallContext.RentalAreas
            .Where(x => x.AreaCode == "1-09")
            .ToListAsync();
        Assert.Single(rentalAreas);
        Assert.Equal("legacy-rental-1", rentalAreas[0].Id);
        Assert.Equal("Rented", rentalAreas[0].Status);
        Assert.Equal("Shop", rentalAreas[0].BusinessType);
    }

    private static MallDbContext CreateMallContext()
    {
        var options = new DbContextOptionsBuilder<MallDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new MallDbContext(options);
    }

    private static UtilityMapDbContext CreateUtilityMapContext()
    {
        var options = new DbContextOptionsBuilder<UtilityMapDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new UtilityMapDbContext(options);
    }

    private sealed class FakeFileStorageService : IFileStorageService
    {
        public Task<string> SaveProfileAvatarAsync(IFormFile file, CancellationToken cancellationToken = default)
            => Task.FromResult("/files/avatar.png");

        public Task<string> SaveCccdImageAsync(IFormFile file, CancellationToken cancellationToken = default)
            => Task.FromResult("/files/cccd.png");

        public Task<string> SaveContractImageAsync(IFormFile file, CancellationToken cancellationToken = default)
            => Task.FromResult("/files/contract.png");

        public Task<string> SaveShopLogoAsync(IFormFile file, CancellationToken cancellationToken = default)
            => Task.FromResult("/files/shop-logo.png");

        public Task<string> SaveShopCoverAsync(IFormFile file, CancellationToken cancellationToken = default)
            => Task.FromResult("/files/shop-cover.png");

        public Task<string> SaveShopProductImageAsync(IFormFile file, CancellationToken cancellationToken = default)
            => Task.FromResult("/files/shop-product.png");
    }
}

using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Application.DTOs;
using ABCDMall.Modules.Users.Application.Services;
using ABCDMall.Modules.Users.Application.Services.RentalAreas;
using ABCDMall.Modules.Users.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace ABCDMall.Modules.Users.Tests;

public class RentalAreaCommandServiceTests
{
    [Fact]
    public async Task CreateRentalAreaAsync_returns_bad_request_because_map_slots_are_the_source_of_truth()
    {
        var service = new RentalAreaCommandService(
            null!,
            null!,
            null!);

        var result = await service.CreateRentalAreaAsync(new CreateRentalAreaDto
        {
            AreaCode = "A1-01",
            Floor = "1",
            AreaName = "North Wing",
            Size = "30m2",
            MonthlyRent = 10000000
        });

        Assert.Equal(ApplicationResultStatus.BadRequest, result.Status);
        Assert.Equal(
            "Rental areas now use Mall Map locations as the single source of truth. Please add or edit slots in the map management module.",
            result.Error);
    }

    [Fact]
    public async Task RegisterTenantAsync_creates_shop_info_for_manager_without_existing_shop()
    {
        var repository = new FakeRentalAreaCommandRepository
        {
            RentalArea = new RentalArea
            {
                Id = "1",
                AreaCode = "1-01",
                Status = "Available"
            },
            Manager = new User
            {
                Id = "users-manager-080",
                Role = "Manager",
                FullName = "Prospect Manager 80",
                CCCD = "089204000080",
                ShopId = null
            }
        };
        var service = new RentalAreaCommandService(
            null!,
            repository,
            new FakeFileStorageService());

        var result = await service.RegisterTenantAsync("1", new RegisterTenantDto
        {
            CCCD = "089204000080",
            Location = "1-01",
            StartDate = DateTime.Today.AddDays(1),
            ElectricityFee = 3500m,
            WaterFee = 15000m,
            ServiceFee = 600000m,
            LeaseTermDays = 180,
            ContractImage = new FormFile(Stream.Null, 0, 0, "contract", "contract.png")
        });

        Assert.Equal(ApplicationResultStatus.Ok, result.Status);
        Assert.NotNull(repository.CreatedShopInfo);
        Assert.Equal("Prospect Manager 80", repository.CreatedShopInfo!.ManagerName);
        Assert.Equal("089204000080", repository.CreatedShopInfo.CCCD);
        Assert.Equal("1-01", repository.CreatedShopInfo.RentalLocation);
        Assert.Equal("users-manager-080", repository.Manager!.Id);
        Assert.Equal(repository.CreatedShopInfo.Id, repository.Manager.ShopId);
        Assert.Single(repository.AddedMonthlyBills);
    }

    private sealed class FakeRentalAreaCommandRepository : IRentalAreaCommandRepository
    {
        public RentalArea? RentalArea { get; set; }
        public User? Manager { get; set; }
        public ShopInfo? ExistingShopInfo { get; set; }
        public ShopInfo? CreatedShopInfo { get; private set; }
        public List<ShopMonthlyBill> AddedMonthlyBills { get; } = [];

        public Task<bool> ExistsRentalAreaByCodeAsync(string normalizedAreaCode, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<RentalArea?> GetRentalAreaByIdAsync(string rentalAreaId, CancellationToken cancellationToken = default)
            => Task.FromResult(RentalArea);

        public Task AddRentalAreaAsync(RentalArea rentalArea, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<User?> GetManagerByCccdAsync(string normalizedCccd, CancellationToken cancellationToken = default)
            => Task.FromResult(Manager);

        public Task<ShopInfo?> GetShopInfoByManagerAsync(User manager, string normalizedCccd, CancellationToken cancellationToken = default)
            => Task.FromResult(ExistingShopInfo);

        public Task AddShopInfoAsync(ShopInfo shopInfo, CancellationToken cancellationToken = default)
        {
            CreatedShopInfo = shopInfo;
            ExistingShopInfo = shopInfo;
            return Task.CompletedTask;
        }

        public Task<ShopInfo?> GetShopInfoByRentalAreaAsync(string rentalLocation, string? tenantName, string? shopInfoId, CancellationToken cancellationToken = default)
            => Task.FromResult<ShopInfo?>(null);

        public Task AddMonthlyBillAsync(ShopMonthlyBill monthlyBill, CancellationToken cancellationToken = default)
        {
            AddedMonthlyBills.Add(monthlyBill);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
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

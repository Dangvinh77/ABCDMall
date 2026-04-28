using ABCDMall.Modules.Users.Application.DTOs.RentalAreas;
using ABCDMall.Modules.Users.Application.DTOs.ShopInfos;
using ABCDMall.Modules.Users.Application.Mappings;
using ABCDMall.Modules.Users.Application.Services.RentalAreas;
using ABCDMall.Modules.Users.Application.Services.ShopInfos;
using ABCDMall.Modules.Users.Domain.Entities;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ABCDMall.Modules.Users.Tests;

public class Phase3QueryServicesTests
{
    [Fact]
    public async Task GetRentalAreaDetailAsync_returns_repository_projection()
    {
        var repository = new FakeRentalAreaReadRepository
        {
            RentalAreaDetail = new RentalAreaDetailResponseDto
            {
                Id = "rental-1",
                AreaCode = "A1",
                Floor = "Floor 1",
                AreaName = "North Wing",
                RentalLocation = "L1-01",
                ShopName = "Fashion Hub"
            }
        };
        var service = new RentalAreaQueryService(CreateMapper(), repository);

        var result = await service.GetRentalAreaDetailAsync("rental-1");

        Assert.NotNull(result);
        Assert.Equal("rental-1", result!.Id);
        Assert.Equal("L1-01", result.RentalLocation);
        Assert.Equal("Fashion Hub", result.ShopName);
    }

    [Fact]
    public async Task GetRentalInfoAsync_maps_shop_info_to_rental_info_response()
    {
        var repository = new FakeShopMonthlyBillReadRepository
        {
            RentalInfo = new ShopInfo
            {
                Id = "shop-info-1",
                ShopName = "Book World",
                ManagerName = "Alice Manager",
                CCCD = "123456789",
                RentalLocation = "B2-09",
                Floor = "Floor 2",
                LeaseStartDate = new DateTime(2026, 4, 1),
                ElectricityFee = 120.5m,
                WaterFee = 30.25m,
                ServiceFee = 80m,
                LeaseTermDays = 365,
                ContractImage = "/contracts/book-world.png"
            }
        };
        var service = new ShopInfoQueryService(CreateMapper(), repository);

        var result = await service.GetRentalInfoAsync("shop-info-1");

        Assert.NotNull(result);
        Assert.Equal("shop-info-1", result!.ShopInfoId);
        Assert.Equal("Book World", result.ShopName);
        Assert.Equal("2026-04-01", result.LeaseStartDate);
        Assert.Equal(365, result.LeaseTermDays);
    }

    [Fact]
    public async Task CheckManagerByCccdAsync_returns_manager_even_when_shop_info_does_not_exist_yet()
    {
        var repository = new FakeRentalAreaReadRepository
        {
            Manager = new User
            {
                Id = "manager-80",
                Role = "Manager",
                FullName = "Prospect Manager 80",
                CCCD = "089204000080"
            }
        };
        var service = new RentalAreaQueryService(CreateMapper(), repository);

        var result = await service.CheckManagerByCccdAsync("089204000080");

        Assert.NotNull(result);
        Assert.Equal("Prospect Manager 80", result!.ManagerName);
        Assert.Equal("089204000080", result.CCCD);
        Assert.Null(result.ShopId);
        Assert.Equal("Prospect Manager 80's Shop", result.ShopName);
    }

    private static AutoMapper.IMapper CreateMapper()
    {
        using var loggerFactory = LoggerFactory.Create(_ => { });
        return new AutoMapper.MapperConfiguration(cfg => cfg.AddProfile<UsersProfile>(), loggerFactory).CreateMapper();
    }

    private sealed class FakeRentalAreaReadRepository : IRentalAreaReadRepository
    {
        public RentalAreaDetailResponseDto? RentalAreaDetail { get; set; }
        public User? Manager { get; set; }

        public Task<IReadOnlyList<RentalArea>> GetRentalAreasAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<RentalArea>>([]);

        public Task<RentalAreaDetailResponseDto?> GetRentalAreaDetailByIdAsync(string rentalAreaId, CancellationToken cancellationToken = default)
            => Task.FromResult(RentalAreaDetail);

        public Task<User?> GetManagerByCccdAsync(string normalizedCccd, CancellationToken cancellationToken = default)
            => Task.FromResult(Manager);

        public Task<ShopInfo?> GetShopInfoByManagerAsync(User manager, string normalizedCccd, CancellationToken cancellationToken = default)
            => Task.FromResult<ShopInfo?>(null);
    }

    private sealed class FakeShopMonthlyBillReadRepository : IShopMonthlyBillReadRepository
    {
        public ShopInfo? RentalInfo { get; set; }

        public Task<IReadOnlyList<ShopMonthlyBill>> GetBillsAsync(string? shopId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ShopMonthlyBill>>([]);

        public Task<ShopInfo?> GetRentalInfoAsync(string? shopId, CancellationToken cancellationToken = default)
            => Task.FromResult(RentalInfo);
    }
}

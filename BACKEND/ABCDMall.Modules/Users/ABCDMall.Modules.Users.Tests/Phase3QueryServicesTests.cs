using ABCDMall.Modules.Users.Application.DTOs.RentalAreas;
using ABCDMall.Modules.Users.Application.DTOs.ShopInfos;
using ABCDMall.Modules.Users.Application.Mappings;
using ABCDMall.Modules.Users.Application.Services.Auth;
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

    [Fact]
    public async Task GetUsersAsync_enriches_manager_business_type_from_rental_areas()
    {
        var repository = new FakeUserReadRepository
        {
            Users =
            [
                new User
                {
                    Id = "manager-1",
                    Email = "food.manager@example.com",
                    Role = "Manager",
                    FullName = "Food Manager",
                    ShopId = "shop-001"
                },
                new User
                {
                    Id = "manager-2",
                    Email = "shop.manager@example.com",
                    Role = "Manager",
                    FullName = "Shop Manager",
                    ShopId = "shop-002"
                }
            ],
            ShopNamesById = new Dictionary<string, string>
            {
                ["shop-001"] = "Ocean Blue Seafood Buffet",
                ["shop-002"] = "Minh Fashion"
            },
            BusinessTypesByShopId = new Dictionary<string, string>
            {
                ["shop-001"] = "FoodCourt",
                ["shop-002"] = "Shop"
            }
        };

        var service = new UserQueryService(CreateMapper(), repository);

        var result = await service.GetUsersAsync();

        Assert.Collection(
            result,
            first =>
            {
                Assert.Equal("Ocean Blue Seafood Buffet", first.ShopName);
                Assert.Equal("FoodCourt", first.BusinessType);
            },
            second =>
            {
                Assert.Equal("Minh Fashion", second.ShopName);
                Assert.Equal("Shop", second.BusinessType);
            });
    }

    [Fact]
    public async Task GetUsersAsync_defaults_movies_admin_business_type_to_movies()
    {
        var repository = new FakeUserReadRepository
        {
            Users =
            [
                new User
                {
                    Id = "movies-admin-1",
                    Email = "movies.admin@example.com",
                    Role = "MoviesAdmin",
                    FullName = "Movies Admin"
                }
            ]
        };
        var service = new UserQueryService(CreateMapper(), repository);

        var result = await service.GetUsersAsync();

        var account = Assert.Single(result);
        Assert.Equal("Movies", account.BusinessType);
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

    private sealed class FakeUserReadRepository : IUserReadRepository
    {
        public IReadOnlyList<User> Users { get; set; } = [];
        public IReadOnlyDictionary<string, string> ShopNamesById { get; set; } = new Dictionary<string, string>();
        public IReadOnlyDictionary<string, string> BusinessTypesByShopId { get; set; } = new Dictionary<string, string>();

        public Task<User?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
            => Task.FromResult<User?>(Users.FirstOrDefault(user => user.Id == userId));

        public Task<IReadOnlyList<ProfileUpdateHistory>> GetProfileUpdateHistoryAsync(string userId, int take, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ProfileUpdateHistory>>([]);

        public Task<IReadOnlyList<ProfileUpdateRequest>> GetProfileUpdateRequestsAsync(string? status, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ProfileUpdateRequest>>([]);

        public Task<IReadOnlyList<ProfileUpdateRequest>> GetProfileUpdateRequestsByUserAsync(string userId, string? status, int take, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ProfileUpdateRequest>>([]);

        public Task<IReadOnlyList<User>> GetUsersAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Users);

        public Task<IReadOnlyList<User>> GetUsersByRoleAsync(string role, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<User>>(Users.Where(user => user.Role == role).ToList());

        public Task<IReadOnlyDictionary<string, string>> GetShopNamesByIdsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(ShopNamesById);

        public Task<IReadOnlyDictionary<string, string>> GetBusinessTypesByShopIdsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(BusinessTypesByShopId);
    }
}

using ABCDMall.Modules.UtilityMap.Application.DTOs.Maps;
using ABCDMall.Modules.UtilityMap.Application.Services.Maps;
using ABCDMall.Modules.UtilityMap.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ABCDMall.Modules.UtilityMap.Tests;

public sealed class MapCommandServiceTests
{
    [Fact]
    public async Task ReserveSlotAsync_AssignsShopInfoIdAndReservedStatus()
    {
        var location = new MapLocation { Id = 7, Status = "Available" };
        var repository = new FakeMapRepository(location);
        var service = new MapCommandService(repository, NullLogger<MapCommandService>.Instance);

        var result = await service.ReserveSlotAsync(7, "shop-123");

        Assert.True(result);
        Assert.Equal("Reserved", location.Status);
        Assert.Equal("shop-123", location.ShopInfoId);
        Assert.Same(location, repository.LastUpdatedLocation);
    }

    [Fact]
    public async Task ReleaseSlotAsync_ClearsShopInfoIdAndRestoresAvailableStatus()
    {
        var location = new MapLocation { Id = 7, Status = "Reserved", ShopInfoId = "shop-123" };
        var repository = new FakeMapRepository(location);
        var service = new MapCommandService(repository, NullLogger<MapCommandService>.Instance);

        var result = await service.ReleaseSlotAsync(7);

        Assert.True(result);
        Assert.Equal("Available", location.Status);
        Assert.Null(location.ShopInfoId);
        Assert.Same(location, repository.LastUpdatedLocation);
    }

    [Fact]
    public async Task UpdateSlotStatusByShopInfoIdAsync_UpdatesExistingAssignedLocation()
    {
        var location = new MapLocation { Id = 9, Status = "Reserved", ShopInfoId = "shop-123" };
        var repository = new FakeMapRepository(location);
        var service = new MapCommandService(repository, NullLogger<MapCommandService>.Instance);

        var result = await service.UpdateSlotStatusByShopInfoIdAsync("shop-123", "ComingSoon");

        Assert.True(result);
        Assert.Equal("ComingSoon", location.Status);
        Assert.Equal("shop-123", location.ShopInfoId);
    }

    private sealed class FakeMapRepository : IMapRepository
    {
        private readonly MapLocation? _location;

        public FakeMapRepository(MapLocation? location)
        {
            _location = location;
        }

        public MapLocation? LastUpdatedLocation { get; private set; }

        public Task<List<FloorPlan>> GetAllFloorsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new List<FloorPlan>());

        public Task<FloorPlan?> GetFloorPlanAsync(string floorLevel, CancellationToken cancellationToken = default)
            => Task.FromResult<FloorPlan?>(null);

        public Task CreateFloorPlanAsync(FloorPlan floorPlan, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<bool> AddLocationAsync(int floorPlanId, MapLocation location, CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task<bool> DeleteLocationAsync(int locationId, CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task<bool> UpdateFloorPlanAsync(int id, FloorPlan floorPlan, CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task<FloorPlan?> GetFloorPlanByIdAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult<FloorPlan?>(null);

        public Task<MapLocation?> GetLocationByIdAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult(_location is not null && _location.Id == id ? _location : null);

        public Task<MapLocation?> GetLocationByShopInfoIdAsync(string shopInfoId, CancellationToken cancellationToken = default)
            => Task.FromResult(_location is not null && _location.ShopInfoId == shopInfoId ? _location : null);

        public Task UpdateLocationSlotAsync(MapLocation location, CancellationToken cancellationToken = default)
        {
            LastUpdatedLocation = location;
            return Task.CompletedTask;
        }

        public Task<bool> UpdateLocationStatusByShopInfoIdAsync(string shopInfoId, string status, CancellationToken cancellationToken = default)
        {
            if (_location is null || !string.Equals(_location.ShopInfoId, shopInfoId, StringComparison.Ordinal))
            {
                return Task.FromResult(false);
            }

            _location.Status = status;
            LastUpdatedLocation = _location;
            return Task.FromResult(true);
        }

        public Task<bool> UpdateLocationDetailsByShopInfoIdAsync(string shopInfoId, string shopName, string shopUrl, CancellationToken cancellationToken = default)
        {
            if (_location is null || !string.Equals(_location.ShopInfoId, shopInfoId, StringComparison.Ordinal))
            {
                return Task.FromResult(false);
            }

            _location.ShopName = shopName;
            _location.ShopUrl = shopUrl;
            LastUpdatedLocation = _location;
            return Task.FromResult(true);
        }
    }
}

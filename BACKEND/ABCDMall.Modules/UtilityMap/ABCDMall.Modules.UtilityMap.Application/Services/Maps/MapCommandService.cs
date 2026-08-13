using ABCDMall.Modules.UtilityMap.Application.DTOs.Maps;
using ABCDMall.Modules.UtilityMap.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ABCDMall.Modules.UtilityMap.Application.Services.Maps;

public sealed class MapCommandService : IMapCommandService
{
    private readonly IMapRepository _repo;
    private readonly ILogger<MapCommandService> _logger;

    public MapCommandService(IMapRepository repo, ILogger<MapCommandService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task CreateFloorPlanAsync(CreateFloorPlanRequestDto request, CancellationToken cancellationToken = default)
    {
        var entity = new FloorPlan
        {
            FloorLevel = request.FloorLevel.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            BlueprintImageUrl = request.BlueprintImageUrl?.Trim() ?? string.Empty
        };

        await _repo.CreateFloorPlanAsync(entity, cancellationToken);
        _logger.LogInformation("Created floor plan {FloorLevel}.", entity.FloorLevel);
    }

    public async Task<bool> UpdateFloorPlanAsync(int id, UpdateFloorPlanRequestDto request, CancellationToken cancellationToken = default)
    {
        var existing = await _repo.GetFloorPlanByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            _logger.LogWarning("Cannot update floor plan {Id} because it does not exist.", id);
            return false;
        }

        existing.Description = request.Description?.Trim() ?? string.Empty;
        existing.BlueprintImageUrl = request.BlueprintImageUrl?.Trim() ?? string.Empty;

        await _repo.UpdateFloorPlanAsync(id, existing, cancellationToken);
        _logger.LogInformation("Updated floor plan {Id}.", id);
        return true;
    }

    public async Task<bool> AddLocationAsync(int floorPlanId, CreateMapLocationRequestDto request, CancellationToken cancellationToken = default)
    {
        var entity = new MapLocation
        {
            ShopName = request.ShopName.Trim(),
            LocationSlot = request.LocationSlot.Trim(),
            ShopUrl = request.ShopUrl?.Trim() ?? string.Empty,
            X = request.X,
            Y = request.Y,
            StorefrontImageUrl = request.StorefrontImageUrl?.Trim() ?? string.Empty
        };

        var result = await _repo.AddLocationAsync(floorPlanId, entity, cancellationToken);
        if (result)
        {
            _logger.LogInformation("Added location {ShopName} to floor plan {FloorPlanId}.", entity.ShopName, floorPlanId);
        }
        else
        {
            _logger.LogWarning("Failed to add location {ShopName} to floor plan {FloorPlanId}.", entity.ShopName, floorPlanId);
        }

        return result;
    }

    public async Task<bool> DeleteLocationAsync(int locationId, CancellationToken cancellationToken = default)
    {
        var result = await _repo.DeleteLocationAsync(locationId, cancellationToken);
        if (result)
        {
            _logger.LogInformation("Deleted map location {LocationId}.", locationId);
        }
        else
        {
            _logger.LogWarning("Cannot delete map location {LocationId} because it does not exist.", locationId);
        }

        return result;
    }

    public async Task<bool> ReserveSlotAsync(int locationId, string shopInfoId, CancellationToken cancellationToken = default)
    {
        var location = await _repo.GetLocationByIdAsync(locationId, cancellationToken);
        if (location is null)
        {
            _logger.LogWarning("Cannot reserve map location {LocationId} because it does not exist.", locationId);
            return false;
        }

        location.Status = "Reserved";
        location.ShopInfoId = shopInfoId;
        await _repo.UpdateLocationSlotAsync(location, cancellationToken);
        _logger.LogInformation("Reserved map location {LocationId} for shop info {ShopInfoId}.", locationId, shopInfoId);
        return true;
    }

    public async Task<bool> ReleaseSlotAsync(int locationId, CancellationToken cancellationToken = default)
    {
        var location = await _repo.GetLocationByIdAsync(locationId, cancellationToken);
        if (location is null)
        {
            _logger.LogWarning("Cannot release map location {LocationId} because it does not exist.", locationId);
            return false;
        }

        location.Status = "Available";
        location.ShopInfoId = null;
        await _repo.UpdateLocationSlotAsync(location, cancellationToken);
        _logger.LogInformation("Released map location {LocationId}.", locationId);
        return true;
    }

    public async Task<bool> UpdateSlotStatusByShopInfoIdAsync(string shopInfoId, string status, CancellationToken cancellationToken = default)
    {
        var normalizedStatus = string.IsNullOrWhiteSpace(status) ? "Active" : status.Trim();
        var result = await _repo.UpdateLocationStatusByShopInfoIdAsync(shopInfoId, normalizedStatus, cancellationToken);

        if (result)
        {
            _logger.LogInformation("Updated map slot status for shop info {ShopInfoId} to {Status}.", shopInfoId, normalizedStatus);
        }
        else
        {
            _logger.LogWarning("Cannot update map slot status for shop info {ShopInfoId} because no mapped location exists.", shopInfoId);
        }

        return result;
    }

    public async Task<bool> UpdateLocationDetailsByShopInfoIdAsync(string shopInfoId, string shopName, string shopUrl, CancellationToken cancellationToken = default)
    {
        var result = await _repo.UpdateLocationDetailsByShopInfoIdAsync(shopInfoId, shopName, shopUrl, cancellationToken);
        if (result)
        {
            _logger.LogInformation("Updated map details for shop info {ShopInfoId} (Name: {ShopName}, Url: {ShopUrl}).", shopInfoId, shopName, shopUrl);
        }
        return result;
    }
}

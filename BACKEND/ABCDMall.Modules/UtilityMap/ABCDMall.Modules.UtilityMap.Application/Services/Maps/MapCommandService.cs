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
    public async Task<bool> ReserveSlotAsync(
        int locationId,
        string shopInfoId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(shopInfoId))
        {
            _logger.LogWarning("ReserveSlot called with empty shopInfoId for location {LocationId}.", locationId);
            return false;
        }

        var location = await _repo.GetLocationByIdAsync(locationId, cancellationToken);
        if (location is null)
        {
            _logger.LogWarning("Cannot reserve location {LocationId}: not found.", locationId);
            return false;
        }

        if (location.Status != "Available")
        {
            _logger.LogWarning(
                "Cannot reserve location {LocationId}: current status is '{Status}'.",
                locationId, location.Status);
            return false;
        }

        var result = await _repo.UpdateLocationSlotAsync(locationId, "Reserved", shopInfoId, cancellationToken);
        if (result)
        {
            _logger.LogInformation(
                "Reserved location {LocationId} (slot '{Slot}') for ShopInfo {ShopInfoId}.",
                locationId, location.LocationSlot, shopInfoId);
        }

        return result;
    }

    public async Task<bool> ReleaseSlotAsync(
        int locationId,
        CancellationToken cancellationToken = default)
    {
        var location = await _repo.GetLocationByIdAsync(locationId, cancellationToken);
        if (location is null)
        {
            _logger.LogWarning("Cannot release location {LocationId}: not found.", locationId);
            return false;
        }

        if (location.Status == "Available")
        {
            _logger.LogWarning("Location {LocationId} is already Available.", locationId);
            return false;
        }

        var result = await _repo.UpdateLocationSlotAsync(locationId, "Available", null, cancellationToken);
        if (result)
        {
            _logger.LogInformation("Released location {LocationId} back to Available.", locationId);
        }

        return result;
    }

    public async Task UpdateSlotStatusByShopInfoIdAsync(
        string shopInfoId,
        string status,
        CancellationToken cancellationToken = default)
    {
        // Bỏ qua nếu không tìm thấy slot (slot assignment là optional)
        var updated = await _repo.UpdateLocationStatusByShopInfoIdAsync(shopInfoId, status, cancellationToken);
        if (updated)
        {
            _logger.LogInformation(
                "Updated map slot status to '{Status}' for ShopInfo {ShopInfoId}.", status, shopInfoId);
        }
        else
        {
            _logger.LogDebug(
                "No map slot found for ShopInfo {ShopInfoId} — status sync skipped.", shopInfoId);
        }
    }
}

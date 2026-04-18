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
}

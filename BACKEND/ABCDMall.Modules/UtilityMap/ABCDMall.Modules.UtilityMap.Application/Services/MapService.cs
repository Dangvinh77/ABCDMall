using ABCDMall.Modules.UtilityMap.Domain.Entities;
using ABCDMall.Modules.UtilityMap.Domain.Interfaces;
using ABCDMall.Shared.DTOs;

namespace ABCDMall.Modules.UtilityMap.Application.Services;

public class MapService : IMapService
{
    private readonly IMapRepository _repo;

    public MapService(IMapRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<FloorPlanDto>> GetAllFloorsAsync()
    {
        var floors = await _repo.GetAllFloorsAsync();
        return floors.Select(MapToDto).ToList();
    }

    public async Task<FloorPlanDto?> GetFloorPlanAsync(string floorLevel)
    {
        var floor = await _repo.GetFloorPlanAsync(floorLevel);
        return floor == null ? null : MapToDto(floor);
    }
    public async Task<bool> UpdateFloorPlanAsync(int id, FloorPlanDto dto)
    {
        return await _repo.UpdateFloorPlanAsync(id, dto.BlueprintImageUrl, dto.Description);
    }
    public async Task CreateFloorPlanAsync(FloorPlanDto dto)
    {
        var entity = new FloorPlan
        {
            FloorLevel = dto.FloorLevel,
            Description = dto.Description,
            BlueprintImageUrl = dto.BlueprintImageUrl
        };
        await _repo.CreateFloorPlanAsync(entity);
    }

    public async Task<bool> AddLocationAsync(int floorPlanId, MapLocationDto dto)
    {
        var entity = new MapLocation
        {
            ShopName = dto.ShopName,
            LocationSlot = dto.LocationSlot,
            ShopUrl = dto.ShopUrl,
            X = dto.X,
            Y = dto.Y,
            StorefrontImageUrl = dto.StorefrontImageUrl
        };
        return await _repo.AddLocationAsync(floorPlanId, entity);
    }

    public async Task<bool> DeleteLocationAsync(int locationId)
        => await _repo.DeleteLocationAsync(locationId);

    // ===== MAPPER =====
    private static FloorPlanDto MapToDto(FloorPlan f) => new()
    {
        Id = f.Id,
        FloorLevel = f.FloorLevel,
        Description = f.Description,
        BlueprintImageUrl = f.BlueprintImageUrl,
        Locations = f.Locations.Select(l => new MapLocationDto
        {
            Id = l.Id,
            ShopName = l.ShopName,
            LocationSlot = l.LocationSlot,
            ShopUrl = l.ShopUrl,
            X = l.X,
            Y = l.Y,
            StorefrontImageUrl = l.StorefrontImageUrl
        }).ToList()
    };
}
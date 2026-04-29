using ABCDMall.Modules.UtilityMap.Application.DTOs.Maps;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace ABCDMall.Modules.UtilityMap.Application.Services.Maps;

public sealed class MapQueryService : IMapQueryService
{
    private readonly IMapRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<MapQueryService> _logger;

    public MapQueryService(IMapRepository repo, IMapper mapper, ILogger<MapQueryService> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<FloorPlanDto>> GetAllFloorsAsync(CancellationToken cancellationToken = default)
    {
        var floors = await _repo.GetAllFloorsAsync(cancellationToken);
        _logger.LogInformation("Fetched {Count} floor plans.", floors.Count);
        return _mapper.Map<IReadOnlyList<FloorPlanDto>>(floors);
    }

    public async Task<FloorPlanDto?> GetFloorPlanAsync(string floorLevel, CancellationToken cancellationToken = default)
    {
        var floor = await _repo.GetFloorPlanAsync(floorLevel, cancellationToken);
        if (floor is null)
        {
            _logger.LogWarning("Floor plan {FloorLevel} was not found.", floorLevel);
            return null;
        }

        return _mapper.Map<FloorPlanDto>(floor);
    }
    public async Task<IReadOnlyList<FloorPlanAdminDto>> GetAllFloorsForAdminAsync(CancellationToken cancellationToken = default)
    {
        var floors = await _repo.GetAllFloorsAsync(cancellationToken);
        _logger.LogInformation("Admin fetched {Count} floor plans with full slot status.", floors.Count);
        return floors.Select(MapToAdminDto).ToList();
    }

    public async Task<FloorPlanAdminDto?> GetFloorPlanForAdminAsync(string floorLevel, CancellationToken cancellationToken = default)
    {
        var floor = await _repo.GetFloorPlanAsync(floorLevel, cancellationToken);
        if (floor is null)
        {
            _logger.LogWarning("Admin: Floor plan {FloorLevel} was not found.", floorLevel);
            return null;
        }

        return MapToAdminDto(floor);
    }

    private static FloorPlanAdminDto MapToAdminDto(
        ABCDMall.Modules.UtilityMap.Domain.Entities.FloorPlan floor)
    {
        return new FloorPlanAdminDto
        {
            Id = floor.Id,
            FloorLevel = floor.FloorLevel,
            Description = floor.Description,
            BlueprintImageUrl = floor.BlueprintImageUrl,
            Locations = floor.Locations.Select(l => new MapLocationAdminDto
            {
                Id = l.Id,
                ShopName = l.ShopName,
                LocationSlot = l.LocationSlot,
                ShopUrl = l.ShopUrl,
                X = l.X,
                Y = l.Y,
                StorefrontImageUrl = l.StorefrontImageUrl,
                Status = l.Status,
                ShopInfoId = l.ShopInfoId
            }).ToList()
        };
    }
}

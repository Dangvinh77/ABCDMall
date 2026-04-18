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
}

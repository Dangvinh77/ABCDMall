namespace ABCDMall.Modules.UtilityMap.Application.DTOs.Maps;

/// <summary>
/// Floor plan DTO dành cho Admin — locations chứa MapLocationAdminDto thay vì MapLocationDto.
/// </summary>
public sealed class FloorPlanAdminDto
{
    public int Id { get; set; }
    public string FloorLevel { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BlueprintImageUrl { get; set; } = string.Empty;
    public IReadOnlyList<MapLocationAdminDto> Locations { get; set; } = [];
}
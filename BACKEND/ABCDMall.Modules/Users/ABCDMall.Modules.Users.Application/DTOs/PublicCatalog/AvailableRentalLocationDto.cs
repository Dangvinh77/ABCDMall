namespace ABCDMall.Modules.Users.Application.DTOs.PublicCatalog;

public sealed class AvailableRentalLocationDto
{
    public string LocationSlot { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public string? AreaName { get; set; }
}

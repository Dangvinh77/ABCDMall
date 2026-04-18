namespace ABCDMall.Modules.Users.Application.DTOs.RentalAreas;

public sealed class ManagerLookupResponseDto
{
    public string? ManagerName { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string? ShopId { get; set; }
    public string CCCD { get; set; } = string.Empty;
}

namespace ABCDMall.Modules.Users.Application.DTOs.PublicCatalog;

public sealed class ShopCreationStatusDto
{
    public int ShopCount { get; set; }
    public int RentedAreaCount { get; set; }
    public bool CanCreate { get; set; }
    public string Message { get; set; } = string.Empty;
    public IReadOnlyList<AvailableRentalLocationDto> AvailableRentalLocations { get; set; } = [];
}

namespace ABCDMall.Modules.FoodCourt.Application.DTOs.Foods;

public sealed class FoodManagerCreationStatusDto
{
    public int StallCount { get; set; }
    public int RentedFoodCourtCount { get; set; }
    public bool CanCreate { get; set; }
    public string Message { get; set; } = string.Empty;
    public IReadOnlyList<AvailableFoodCourtLocationDto> AvailableRentalLocations { get; set; } = [];
}

public sealed class AvailableFoodCourtLocationDto
{
    public string RentalAreaId { get; set; } = string.Empty;
    public string LocationSlot { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public string AreaName { get; set; } = string.Empty;
}

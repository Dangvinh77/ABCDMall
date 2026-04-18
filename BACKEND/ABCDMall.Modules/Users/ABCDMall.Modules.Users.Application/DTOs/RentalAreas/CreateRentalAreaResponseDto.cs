namespace ABCDMall.Modules.Users.Application.DTOs.RentalAreas;

public sealed class CreateRentalAreaResponseDto
{
    public string Message { get; set; } = string.Empty;

    public RentalAreaResponseDto RentalArea { get; set; } = new();
}

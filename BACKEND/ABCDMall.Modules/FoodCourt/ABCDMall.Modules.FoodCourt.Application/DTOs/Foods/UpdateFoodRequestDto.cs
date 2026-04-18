namespace ABCDMall.Modules.FoodCourt.Application.DTOs.Foods;

public sealed class UpdateFoodRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}

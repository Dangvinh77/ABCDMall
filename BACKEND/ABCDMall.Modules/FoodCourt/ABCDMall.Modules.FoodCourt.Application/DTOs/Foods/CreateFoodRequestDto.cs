using Microsoft.AspNetCore.Http;

namespace ABCDMall.Modules.FoodCourt.Application.DTOs.Foods;

public sealed class CreateFoodRequestDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public IFormFile? ImageFile { get; set; }
}

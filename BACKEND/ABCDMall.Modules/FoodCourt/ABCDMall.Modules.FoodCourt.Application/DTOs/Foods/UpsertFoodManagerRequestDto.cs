using Microsoft.AspNetCore.Http;

namespace ABCDMall.Modules.FoodCourt.Application.DTOs.Foods;

public sealed class UpsertFoodManagerRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public IFormFile? ImageFile { get; set; }
    public string CategorySlug { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? OpenHours { get; set; }
    public string? Phone { get; set; }
    public string? Promo { get; set; }
    public bool IsActive { get; set; } = true;
    public IReadOnlyList<UpsertFoodMenuItemRequestDto> MenuItems { get; set; } = [];
}

namespace ABCDMall.Modules.FoodCourt.Application.DTOs;

public class FoodItemDto
{
    public string? Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }  // dùng khi nhập link

    public string? Slug { get; set; }

    public string? Description { get; set; }

    public string? CategorySlug { get; set; }

    public string? Location { get; set; }

    public string? OpenHours { get; set; }

    public string? Phone { get; set; }

    public string? Promo { get; set; }
}

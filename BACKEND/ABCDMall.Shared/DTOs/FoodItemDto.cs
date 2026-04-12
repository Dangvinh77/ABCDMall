namespace ABCDMall.Shared.DTOs;

public class FoodItemDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }  // dùng khi nhập link

    public string? Slug { get; set; }

    public string? Description { get; set; }
}
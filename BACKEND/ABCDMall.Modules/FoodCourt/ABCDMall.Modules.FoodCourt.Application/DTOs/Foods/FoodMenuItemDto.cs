namespace ABCDMall.Modules.FoodCourt.Application.DTOs.Foods;

public sealed class FoodMenuItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Note { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public IReadOnlyList<string> Ingredients { get; set; } = [];
    public bool IsAvailable { get; set; }
    public int DisplayOrder { get; set; }
}

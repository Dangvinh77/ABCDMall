namespace ABCDMall.Modules.FoodCourt.Application.DTOs.Foods;

public sealed class UpsertFoodMenuItemRequestDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Note { get; set; }
    public string? Tag { get; set; }
    public string? ImageUrl { get; set; }
    public IReadOnlyList<string> Ingredients { get; set; } = [];
    public bool IsAvailable { get; set; } = true;
    public int DisplayOrder { get; set; }
}

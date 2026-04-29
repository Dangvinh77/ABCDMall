namespace ABCDMall.Modules.FoodCourt.Domain.Entities;

public class FoodMenuItem
{
    public string Id { get; set; } = string.Empty;
    public string FoodStallId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Note { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string IngredientsJson { get; set; } = "[]";
    public bool IsAvailable { get; set; } = true;
    public int DisplayOrder { get; set; }

    public FoodItem? FoodStall { get; set; }
}

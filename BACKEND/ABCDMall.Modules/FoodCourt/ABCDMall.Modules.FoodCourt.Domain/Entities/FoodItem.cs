namespace ABCDMall.Modules.FoodCourt.Domain.Entities;

public class FoodItem
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    //public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? Slug { get; set; }
    public string? MallSlug { get; set; } = "ABCD Mall";
    public string? CategorySlug { get; set; } = "Floor";
    public string Description { get; set; } = string.Empty;
}

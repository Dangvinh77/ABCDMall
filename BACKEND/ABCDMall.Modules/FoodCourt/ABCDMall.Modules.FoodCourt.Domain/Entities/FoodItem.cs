namespace ABCDMall.Modules.FoodCourt.Domain.Entities;

public class FoodItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    //public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string MallSlug { get; set; } = "ABCD Mall";
    public string CategorySlug { get; set; } = "Floor";
    public string Description { get; set; } = string.Empty;
}

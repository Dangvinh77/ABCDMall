namespace ABCDMall.Modules.FoodCourt.Domain.Entities;

public class FoodItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Slug { get; set; }
    public string? MallSlug { get; set; } = "ABCD Mall";
    public string? CategorySlug { get; set; } = "Floor";
    public string Description { get; set; } = string.Empty;
}
namespace ABCDMall.Modules.FoodCourt.Domain.Entities;

public class FoodItem
{
    public string Id { get; set; } = string.Empty;
    public string OwnerShopId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string MallSlug { get; set; } = "ABCD Mall";
    public string CategorySlug { get; set; } = "Floor";
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string OpenHours { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Promo { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<FoodMenuItem> MenuItems { get; set; } = [];
}


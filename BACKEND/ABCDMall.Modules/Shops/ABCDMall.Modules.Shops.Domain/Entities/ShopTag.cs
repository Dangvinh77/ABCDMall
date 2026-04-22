namespace ABCDMall.Modules.Shops.Domain.Entities;

public sealed class ShopTag
{
    public string Id { get; set; } = string.Empty;
    public string ShopId { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public Shop Shop { get; set; } = null!;
}

namespace ABCDMall.Modules.Shops.Domain.Entities;

public sealed class ShopVoucher
{
    public string Id { get; set; } = string.Empty;
    public string ShopId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ValidUntil { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public Shop Shop { get; set; } = null!;
}

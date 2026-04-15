namespace ABCDMall.Modules.Shops.Domain.Entities;

public class Voucher
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ShopId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ValidUntil { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Navigation Property
    public Shop Shop { get; set; } = null!;
}
namespace ABCDMall.Modules.Shops.Application.DTOs;

public sealed class ShopDetailDto : ShopCatalogItemDto
{
    public string CoverImageUrl { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public string LocationSlot { get; set; } = string.Empty;
    public IReadOnlyList<ShopProductDto> Products { get; set; } = [];
    public IReadOnlyList<ShopVoucherDto> Vouchers { get; set; } = [];
}

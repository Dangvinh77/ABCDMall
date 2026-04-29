namespace ABCDMall.Modules.Users.Application.Services.Bidding;

public sealed class ShopAdTemplateData
{
    public string ShopImage { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}

public sealed class DiscountAdTemplateData
{
    public string ProductImage { get; set; } = string.Empty;

    public decimal OriginalPrice { get; set; }

    public decimal DiscountPrice { get; set; }
}

public sealed class EventAdTemplateData
{
    public string EventImage { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public string StartTime { get; set; } = string.Empty;
}

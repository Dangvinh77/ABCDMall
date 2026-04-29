namespace ABCDMall.Modules.Users.Application.Services.Bidding;

public sealed class BidStripeCheckoutSessionRequest
{
    public string BidId { get; init; } = string.Empty;

    public string ShopId { get; init; } = string.Empty;

    public string ShopName { get; init; } = string.Empty;

    public string TemplateType { get; init; } = string.Empty;

    public decimal BidAmount { get; init; }

    public string Currency { get; init; } = "USD";

    public string? CustomerEmail { get; init; }

    public DateTime ExpiresAtUtc { get; init; }
}

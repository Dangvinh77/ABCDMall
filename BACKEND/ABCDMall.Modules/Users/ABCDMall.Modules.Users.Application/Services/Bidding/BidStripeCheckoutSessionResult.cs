namespace ABCDMall.Modules.Users.Application.Services.Bidding;

public sealed class BidStripeCheckoutSessionResult
{
    public string SessionId { get; init; } = string.Empty;

    public string CheckoutUrl { get; init; } = string.Empty;

    public DateTime ExpiresAtUtc { get; init; }
}

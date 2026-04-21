namespace ABCDMall.Modules.Movies.Application.Services.Payments;

public sealed class StripeCheckoutSessionResult
{
    public string SessionId { get; init; } = string.Empty;
    public string CheckoutUrl { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
}

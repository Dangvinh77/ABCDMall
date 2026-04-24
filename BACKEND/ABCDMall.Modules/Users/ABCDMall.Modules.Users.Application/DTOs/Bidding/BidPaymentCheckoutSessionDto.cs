namespace ABCDMall.Modules.Users.Application.DTOs.Bidding;

public sealed class BidPaymentCheckoutSessionDto
{
    public string BidId { get; set; } = string.Empty;

    public string SessionId { get; set; } = string.Empty;

    public string CheckoutUrl { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }
}

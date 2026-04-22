namespace ABCDMall.Modules.Movies.Application.DTOs.Payments;

public sealed class StripeCheckoutSessionResponseDto
{
    public Guid BookingId { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public Guid? HoldId { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string CheckoutUrl { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}

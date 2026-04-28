namespace ABCDMall.Modules.Users.Infrastructure.Services.RentalPayments;

public sealed class StripeCheckoutSessionResult
{
    public string SessionId { get; set; } = string.Empty;

    public string CheckoutUrl { get; set; } = string.Empty;
}

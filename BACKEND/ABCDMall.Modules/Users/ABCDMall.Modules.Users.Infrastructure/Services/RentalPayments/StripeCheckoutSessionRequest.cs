namespace ABCDMall.Modules.Users.Infrastructure.Services.RentalPayments;

public sealed class StripeCheckoutSessionRequest
{
    public string SuccessUrl { get; set; } = string.Empty;

    public string CancelUrl { get; set; } = string.Empty;

    public string? CustomerEmail { get; set; }

    public string Currency { get; set; } = "vnd";

    public decimal Amount { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Dictionary<string, string> Metadata { get; set; } = [];
}

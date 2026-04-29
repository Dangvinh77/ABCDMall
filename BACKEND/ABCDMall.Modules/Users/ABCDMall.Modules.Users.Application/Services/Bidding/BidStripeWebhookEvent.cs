namespace ABCDMall.Modules.Users.Application.Services.Bidding;

public sealed class BidStripeWebhookEvent
{
    public string EventType { get; init; } = string.Empty;

    public string? BidId { get; init; }

    public string? ShopId { get; init; }

    public string? ProviderTransactionId { get; init; }

    public decimal? Amount { get; init; }

    public string? Currency { get; init; }

    public string? RawPayload { get; init; }
}

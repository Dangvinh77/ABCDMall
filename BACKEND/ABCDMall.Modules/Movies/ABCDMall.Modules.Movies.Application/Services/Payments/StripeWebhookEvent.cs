namespace ABCDMall.Modules.Movies.Application.Services.Payments;

public sealed class StripeWebhookEvent
{
    public string EventType { get; init; } = string.Empty;
    public Guid? BookingId { get; init; }
    public Guid? HoldId { get; init; }
    public string? ProviderTransactionId { get; init; }
    public decimal? Amount { get; init; }
    public string? Currency { get; init; }
    public string RawPayload { get; init; } = string.Empty;
}

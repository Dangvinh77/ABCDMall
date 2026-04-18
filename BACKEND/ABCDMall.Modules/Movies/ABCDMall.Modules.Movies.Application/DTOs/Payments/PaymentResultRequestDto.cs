namespace ABCDMall.Modules.Movies.Application.DTOs.Payments;

public sealed class PaymentResultRequestDto
{
    public string Provider { get; set; } = string.Empty;
    public string ProviderTransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public string? RawPayload { get; set; }
    public string? FailureReason { get; set; }
}

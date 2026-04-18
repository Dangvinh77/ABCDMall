namespace ABCDMall.Modules.Movies.Application.DTOs.Payments;

public sealed class PaymentStatusResponseDto
{
    public Guid PaymentId { get; set; }
    public Guid BookingId { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string BookingStatus { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public DateTime? CompletedAtUtc { get; set; }
}

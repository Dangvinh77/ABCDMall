namespace ABCDMall.Modules.Users.Application.DTOs.RentalPayments;

public sealed class ConfirmRentalPaymentResponseDto
{
    public string BillId { get; set; } = string.Empty;

    public string PaymentStatus { get; set; } = string.Empty;

    public DateTime? PaidAtUtc { get; set; }
}

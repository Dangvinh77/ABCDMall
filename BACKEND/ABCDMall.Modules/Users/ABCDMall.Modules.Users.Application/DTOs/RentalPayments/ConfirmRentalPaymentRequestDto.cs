namespace ABCDMall.Modules.Users.Application.DTOs.RentalPayments;

public sealed class ConfirmRentalPaymentRequestDto
{
    public string BillId { get; set; } = string.Empty;

    public string SessionId { get; set; } = string.Empty;
}

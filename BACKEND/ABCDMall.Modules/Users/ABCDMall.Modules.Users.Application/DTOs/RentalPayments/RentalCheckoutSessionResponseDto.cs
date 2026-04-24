namespace ABCDMall.Modules.Users.Application.DTOs.RentalPayments;

public sealed class RentalCheckoutSessionResponseDto
{
    public string BillId { get; set; } = string.Empty;

    public string SessionId { get; set; } = string.Empty;

    public string CheckoutUrl { get; set; } = string.Empty;

    public string PaymentStatus { get; set; } = "Unpaid";
}

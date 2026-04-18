namespace ABCDMall.Modules.Movies.Application.DTOs.Bookings;

public sealed class CreateBookingResponseDto
{
    public Guid BookingId { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public Guid ShowtimeId { get; set; }
    public Guid HoldId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public string Currency { get; set; } = "VND";
    public bool PaymentRequired { get; set; } = true;
}

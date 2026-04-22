namespace ABCDMall.Modules.Movies.Application.DTOs.Bookings;

public sealed class CreateBookingResponseDto
{
    public Guid BookingId { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public Guid ShowtimeId { get; set; }
    public IReadOnlyCollection<Guid> HoldIds { get; set; } = Array.Empty<Guid>();
    public Guid HoldId
    {
        get => HoldIds.FirstOrDefault();
        set => HoldIds = value == Guid.Empty ? Array.Empty<Guid>() : new[] { value };
    }
    public string Status { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public string Currency { get; set; } = "VND";
    public bool PaymentRequired { get; set; } = true;
}

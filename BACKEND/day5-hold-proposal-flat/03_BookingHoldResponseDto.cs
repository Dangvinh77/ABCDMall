namespace ABCDMall.Modules.Movies.Application.DTOs.Bookings;

public sealed class BookingHoldResponseDto
{
    public Guid HoldId { get; set; }
    public string HoldCode { get; set; } = string.Empty;
    public Guid ShowtimeId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public int RemainingSeconds { get; set; }
    public decimal SeatSubtotal { get; set; }
    public decimal ComboSubtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public IReadOnlyCollection<BookingHoldSeatResponseDto> Seats { get; set; } = Array.Empty<BookingHoldSeatResponseDto>();
}

namespace ABCDMall.Modules.Movies.Application.DTOs.Bookings;

public sealed class BookingDetailResponseDto
{
    public Guid BookingId { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public Guid ShowtimeId { get; set; }
    public Guid? HoldId { get; set; }
    public Guid? GuestCustomerId { get; set; }
    public Guid? PromotionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhoneNumber { get; set; } = string.Empty;
    public decimal SeatSubtotal { get; set; }
    public decimal ComboSubtotal { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public string Currency { get; set; } = "VND";
    public string? PromotionSnapshotJson { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public IReadOnlyCollection<BookingItemResponseDto> Items { get; set; } = Array.Empty<BookingItemResponseDto>();
}

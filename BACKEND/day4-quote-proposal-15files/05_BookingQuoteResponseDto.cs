namespace ABCDMall.Modules.Movies.Application.DTOs.Bookings;

public sealed class BookingQuoteResponseDto
{
    public Guid ShowtimeId { get; set; }
    public decimal SeatSubtotal { get; set; }
    public decimal ServiceFeeTotal { get; set; }
    public decimal ComboSubtotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public BookingQuotePromotionDto? Promotion { get; set; }
    public IReadOnlyCollection<BookingQuoteLineDto> Lines { get; set; } = Array.Empty<BookingQuoteLineDto>();
}

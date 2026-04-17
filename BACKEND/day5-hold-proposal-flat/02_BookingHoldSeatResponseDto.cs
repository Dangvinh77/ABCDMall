namespace ABCDMall.Modules.Movies.Application.DTOs.Bookings;

public sealed class BookingHoldSeatResponseDto
{
    public Guid SeatInventoryId { get; set; }
    public string SeatCode { get; set; } = string.Empty;
    public string SeatType { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public string? CoupleGroupCode { get; set; }
}

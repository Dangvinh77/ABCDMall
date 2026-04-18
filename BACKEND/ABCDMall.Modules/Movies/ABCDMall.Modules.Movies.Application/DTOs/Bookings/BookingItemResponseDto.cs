namespace ABCDMall.Modules.Movies.Application.DTOs.Bookings;

public sealed class BookingItemResponseDto
{
    public Guid Id { get; set; }
    public string ItemType { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? SeatInventoryId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

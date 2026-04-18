namespace ABCDMall.Modules.Movies.Application.DTOs.Bookings;

public sealed class BookingHoldComboSnapshotDto
{
    public Guid ComboId { get; set; }
    public string ComboCode { get; set; } = string.Empty;
    public string ComboName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

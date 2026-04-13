using ABCDMall.Modules.Movies.Domain.Enums;

namespace ABCDMall.Modules.Movies.Domain.Entities;

public class ShowtimeSeatInventory
{
    public Guid Id { get; set; }
    public Guid ShowtimeId { get; set; }
    public Guid HallSeatId { get; set; }
    public string SeatCode { get; set; } = string.Empty;
    public string RowLabel { get; set; } = string.Empty;
    public int ColumnNumber { get; set; }
    public SeatType SeatType { get; set; } = SeatType.Regular;
    public string? CoupleGroupCode { get; set; }
    public decimal Price { get; set; }
    public SeatInventoryStatus Status { get; set; } = SeatInventoryStatus.Available;
    public DateTime UpdatedAtUtc { get; set; }

    public Showtime? Showtime { get; set; }
    public HallSeat? HallSeat { get; set; }
}

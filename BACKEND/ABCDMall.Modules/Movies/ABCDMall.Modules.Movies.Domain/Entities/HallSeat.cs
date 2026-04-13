using ABCDMall.Modules.Movies.Domain.Enums;

namespace ABCDMall.Modules.Movies.Domain.Entities;

public class HallSeat
{
    public Guid Id { get; set; }
    public Guid HallId { get; set; }
    public string SeatCode { get; set; } = string.Empty;
    public string RowLabel { get; set; } = string.Empty;
    public int ColumnNumber { get; set; }
    public SeatType SeatType { get; set; } = SeatType.Regular;
    public string? CoupleGroupCode { get; set; }
    public bool IsActive { get; set; } = true;

    public Hall? Hall { get; set; }
    public ICollection<ShowtimeSeatInventory> ShowtimeSeatInventories { get; set; } = new List<ShowtimeSeatInventory>();
}

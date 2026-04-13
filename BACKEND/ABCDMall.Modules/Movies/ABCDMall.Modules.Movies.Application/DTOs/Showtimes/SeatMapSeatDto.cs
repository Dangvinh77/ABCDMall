namespace ABCDMall.Modules.Movies.Application.DTOs.Showtimes;

public class SeatMapSeatDto
{
    public Guid SeatInventoryId { get; set; }
    public string SeatCode { get; set; } = string.Empty;
    public string Row { get; set; } = string.Empty;
    public int Col { get; set; }
    public string SeatType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? CoupleGroupCode { get; set; }
}

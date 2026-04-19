namespace ABCDMall.Modules.Movies.Application.DTOs.Showtimes;

public class SeatMapResponseDto
{
    public Guid ShowtimeId { get; set; }
    public Guid HallId { get; set; }
    public string HallType { get; set; } = string.Empty;
    public bool IsBookable { get; set; }
    public string? BookingUnavailableReason { get; set; }
    public IEnumerable<SeatMapSeatDto> Seats { get; set; } = Array.Empty<SeatMapSeatDto>();
}

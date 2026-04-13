namespace ABCDMall.Modules.Movies.Application.DTOs.Showtimes;

public class SeatMapResponseDto
{
    public Guid ShowtimeId { get; set; }
    public Guid HallId { get; set; }
    public string HallType { get; set; } = string.Empty;
    public IEnumerable<SeatMapSeatDto> Seats { get; set; } = Array.Empty<SeatMapSeatDto>();
}

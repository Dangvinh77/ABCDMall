namespace ABCDMall.Modules.Movies.Application.DTOs.Showtimes;

using ABCDMall.Modules.Movies.Application.DTOs.Promotions;

public class SeatMapResponseDto
{
    public Guid ShowtimeId { get; set; }
    public Guid HallId { get; set; }
    public string HallType { get; set; } = string.Empty;
    public bool IsBookable { get; set; }
    public string? BookingUnavailableReason { get; set; }
    public IReadOnlyCollection<PromotionResponseDto> Promotions { get; set; } = Array.Empty<PromotionResponseDto>();
    public IEnumerable<SeatMapSeatDto> Seats { get; set; } = Array.Empty<SeatMapSeatDto>();
}

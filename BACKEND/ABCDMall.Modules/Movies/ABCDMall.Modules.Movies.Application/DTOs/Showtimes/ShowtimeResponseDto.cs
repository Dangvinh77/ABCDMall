namespace ABCDMall.Modules.Movies.Application.DTOs.Showtimes;

public class ShowtimeResponseDto
{
    public Guid ShowtimeId { get; set; }
    public Guid MovieId { get; set; }
    public Guid CinemaId { get; set; }
    public Guid HallId { get; set; }
    public string HallType { get; set; } = string.Empty;
    public DateOnly BusinessDate { get; set; }
    public DateTime StartAtUtc { get; set; }
    public DateTime? EndAtUtc { get; set; }
    public string Language { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string Status { get; set; } = string.Empty;
}

namespace ABCDMall.Modules.Movies.Application.DTOs.Showtimes;

public class ShowtimeListItemDto
{
    public Guid ShowtimeId { get; set; }
    public Guid MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public Guid CinemaId { get; set; }
    public string CinemaName { get; set; } = string.Empty;
    public Guid HallId { get; set; }
    public string HallName { get; set; } = string.Empty;
    public string HallType { get; set; } = string.Empty;
    public DateOnly BusinessDate { get; set; }
    public DateTime StartAtUtc { get; set; }
    public string Language { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string Status { get; set; } = string.Empty;
}

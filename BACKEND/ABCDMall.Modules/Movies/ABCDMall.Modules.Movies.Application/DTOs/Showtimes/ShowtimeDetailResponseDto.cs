namespace ABCDMall.Modules.Movies.Application.DTOs.Showtimes;

public class ShowtimeDetailResponseDto
{
    public Guid ShowtimeId { get; set; }
    public Guid MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string MovieSlug { get; set; } = string.Empty;
    public string? MoviePosterUrl { get; set; }
    public Guid CinemaId { get; set; }
    public string CinemaCode { get; set; } = string.Empty;
    public string CinemaName { get; set; } = string.Empty;
    public Guid HallId { get; set; }
    public string HallCode { get; set; } = string.Empty;
    public string HallName { get; set; } = string.Empty;
    public string HallType { get; set; } = string.Empty;
    public DateOnly BusinessDate { get; set; }
    public DateTime StartAtUtc { get; set; }
    public DateTime? EndAtUtc { get; set; }
    public string Language { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string Status { get; set; } = string.Empty;
}

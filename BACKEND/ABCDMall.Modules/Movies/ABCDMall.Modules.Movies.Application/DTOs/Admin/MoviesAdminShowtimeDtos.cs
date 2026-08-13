namespace ABCDMall.Modules.Movies.Application.DTOs.Admin;

public sealed class MoviesAdminShowtimeListItemDto
{
    public Guid Id { get; set; }
    public Guid MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public Guid CinemaId { get; set; }
    public string CinemaName { get; set; } = string.Empty;
    public Guid HallId { get; set; }
    public string HallName { get; set; } = string.Empty;
    public DateOnly BusinessDate { get; set; }
    public DateTime StartAtUtc { get; set; }
    public DateTime? EndAtUtc { get; set; }
    public string Language { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class MoviesAdminShowtimeUpsertDto
{
    public Guid MovieId { get; set; }
    public Guid CinemaId { get; set; }
    public Guid HallId { get; set; }
    public DateOnly BusinessDate { get; set; }
    public DateTime StartAtUtc { get; set; }
    public decimal BasePrice { get; set; }
    public string Language { get; set; } = "Subtitle";
    public string Status { get; set; } = "Open";
}

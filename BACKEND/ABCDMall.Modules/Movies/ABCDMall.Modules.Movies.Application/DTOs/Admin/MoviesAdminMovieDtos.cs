namespace ABCDMall.Modules.Movies.Application.DTOs.Admin;

public sealed class MoviesAdminMovieListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Synopsis { get; set; }
    public int DurationMinutes { get; set; }
    public string? PosterUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    public string? RatingLabel { get; set; }
    public string DefaultLanguage { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ShowtimeCount { get; set; }
}

public sealed class MoviesAdminMovieUpsertDto
{
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Synopsis { get; set; }
    public int DurationMinutes { get; set; }
    public string? PosterUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    public string? RatingLabel { get; set; }
    public string DefaultLanguage { get; set; } = "Subtitle";
    public string Status { get; set; } = "Draft";
}

namespace ABCDMall.Modules.Movies.Application.DTOs.Movies;

public class MovieListItemResponseDto
{
    public Guid MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public int DurationMinutes { get; set; }
    public string? RatingLabel { get; set; }
    public string Status { get; set; } = string.Empty;
    public IEnumerable<string> Genres { get; set; } = Array.Empty<string>();
}

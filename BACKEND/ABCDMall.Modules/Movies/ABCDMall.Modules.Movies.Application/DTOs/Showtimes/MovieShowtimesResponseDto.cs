namespace ABCDMall.Modules.Movies.Application.DTOs.Showtimes;

public class MovieShowtimesResponseDto
{
    public Guid MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public IReadOnlyList<MovieShowtimeDateGroupDto> Dates { get; set; } = [];
}

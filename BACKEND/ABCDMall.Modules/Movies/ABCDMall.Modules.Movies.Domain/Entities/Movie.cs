using ABCDMall.Modules.Movies.Domain.Enums;

namespace ABCDMall.Modules.Movies.Domain.Entities;

public class Movie
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
    public LanguageType DefaultLanguage { get; set; } = LanguageType.Subtitle;
    public MovieStatus Status { get; set; } = MovieStatus.Draft;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
    public ICollection<MovieCredit> Credits { get; set; } = new List<MovieCredit>();
    public ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
}

namespace ABCDMall.Modules.Movies.Application.DTOs.Movies;

public class MovieHomeResponseDto
{
    public IReadOnlyList<MovieListItemResponseDto> FeaturedMovies { get; set; } = [];
    public IReadOnlyList<MovieListItemResponseDto> NowShowing { get; set; } = [];
    public IReadOnlyList<MovieListItemResponseDto> ComingSoon { get; set; } = [];
    public IReadOnlyList<MovieHomeCinemaDto> TopCinemas { get; set; } = [];
}

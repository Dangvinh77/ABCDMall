namespace ABCDMall.Modules.Movies.Application.DTOs.Showtimes;

public class MovieShowtimeDateGroupDto
{
    public DateOnly BusinessDate { get; set; }
    public IReadOnlyList<MovieCinemaShowtimesDto> Cinemas { get; set; } = [];
}

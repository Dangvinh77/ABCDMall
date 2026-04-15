namespace ABCDMall.Modules.Movies.Application.DTOs.Movies;

public class MovieHomeCinemaDto
{
    public Guid CinemaId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int UpcomingShowtimeCount { get; set; }
}

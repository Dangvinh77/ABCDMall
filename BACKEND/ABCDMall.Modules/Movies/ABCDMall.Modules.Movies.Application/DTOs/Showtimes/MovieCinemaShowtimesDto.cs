namespace ABCDMall.Modules.Movies.Application.DTOs.Showtimes;

public class MovieCinemaShowtimesDto
{
    public Guid CinemaId { get; set; }
    public string CinemaCode { get; set; } = string.Empty;
    public string CinemaName { get; set; } = string.Empty;
    public IReadOnlyList<ShowtimeResponseDto> Showtimes { get; set; } = [];
}

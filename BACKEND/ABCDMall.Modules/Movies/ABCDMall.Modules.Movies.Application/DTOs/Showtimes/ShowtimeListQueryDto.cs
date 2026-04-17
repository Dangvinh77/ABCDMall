namespace ABCDMall.Modules.Movies.Application.DTOs.Showtimes;

public sealed class ShowtimeListQueryDto
{
    public Guid? MovieId { get; init; }
    public Guid? CinemaId { get; init; }
    public DateOnly? BusinessDate { get; init; }
    public string? HallType { get; init; }
    public string? Language { get; init; }
}

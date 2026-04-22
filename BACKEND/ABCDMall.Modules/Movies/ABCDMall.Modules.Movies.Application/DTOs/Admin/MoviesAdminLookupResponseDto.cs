namespace ABCDMall.Modules.Movies.Application.DTOs.Admin;

public sealed class MoviesAdminLookupResponseDto
{
    public IReadOnlyList<MoviesAdminLookupItemDto> Movies { get; set; } = [];
    public IReadOnlyList<MoviesAdminLookupItemDto> Cinemas { get; set; } = [];
    public IReadOnlyList<MoviesAdminHallLookupItemDto> Halls { get; set; } = [];
}

public sealed class MoviesAdminLookupItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class MoviesAdminHallLookupItemDto
{
    public Guid Id { get; set; }
    public Guid CinemaId { get; set; }
    public string Name { get; set; } = string.Empty;
}

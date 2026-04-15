using ABCDMall.Modules.Movies.Application.DTOs.Movies;
using ABCDMall.Modules.Movies.Application.DTOs.Showtimes;
using ABCDMall.Modules.Movies.Application.Services.Movies;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers;

[ApiController]
[Route("api/movies")]
public sealed class MoviesController : ControllerBase
{
    private readonly IMovieQueryService _movieQueryService;

    public MoviesController(IMovieQueryService movieQueryService)
    {
        _movieQueryService = movieQueryService;
    }

    [HttpGet("home")]
    public async Task<ActionResult<MovieHomeResponseDto>> GetHome(CancellationToken cancellationToken = default)
    {
        var response = await _movieQueryService.GetHomeAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MovieListItemResponseDto>>> GetMovies(
        [FromQuery] string? status,
        CancellationToken cancellationToken = default)
    {
        var response = await _movieQueryService.GetListAsync(status, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{movieId:guid}")]
    public async Task<ActionResult<MovieDetailResponseDto>> GetMovieById(
        Guid movieId,
        CancellationToken cancellationToken = default)
    {
        var response = await _movieQueryService.GetByIdAsync(movieId, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpGet("{movieId:guid}/showtimes")]
    public async Task<ActionResult<MovieShowtimesResponseDto>> GetMovieShowtimes(
        Guid movieId,
        [FromQuery] DateOnly? businessDate,
        CancellationToken cancellationToken = default)
    {
        var response = await _movieQueryService.GetShowtimesByMovieIdAsync(movieId, businessDate, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }
}

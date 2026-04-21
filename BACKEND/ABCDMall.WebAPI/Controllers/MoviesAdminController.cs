using ABCDMall.Modules.Movies.Application.DTOs.Admin;
using ABCDMall.Modules.Movies.Application.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers;

[ApiController]
[Authorize(Roles = "MoviesAdmin,Admin")]
[Route("api/movies/admin")]
public sealed class MoviesAdminController : ControllerBase
{
    private readonly IMoviesAdminService _moviesAdminService;

    public MoviesAdminController(IMoviesAdminService moviesAdminService)
    {
        _moviesAdminService = moviesAdminService;
    }

    [HttpGet("lookups")]
    public async Task<ActionResult<MoviesAdminLookupResponseDto>> GetLookups(CancellationToken cancellationToken = default)
        => Ok(await _moviesAdminService.GetLookupsAsync(cancellationToken));

    [HttpGet("movies")]
    public async Task<ActionResult<IReadOnlyList<MoviesAdminMovieListItemDto>>> GetMovies(CancellationToken cancellationToken = default)
        => Ok(await _moviesAdminService.GetMoviesAsync(cancellationToken));

    [HttpGet("movies/{movieId:guid}")]
    public async Task<ActionResult<MoviesAdminMovieListItemDto>> GetMovieById(Guid movieId, CancellationToken cancellationToken = default)
    {
        var movie = await _moviesAdminService.GetMovieByIdAsync(movieId, cancellationToken);
        return movie is null ? NotFound() : Ok(movie);
    }

    [HttpPost("movies")]
    public async Task<ActionResult<MoviesAdminMovieListItemDto>> CreateMovie(
        [FromBody] MoviesAdminMovieUpsertDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var movie = await _moviesAdminService.CreateMovieAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetMovieById), new { movieId = movie.Id }, movie);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Unable to create movie.",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    [HttpPut("movies/{movieId:guid}")]
    public async Task<ActionResult<MoviesAdminMovieListItemDto>> UpdateMovie(
        Guid movieId,
        [FromBody] MoviesAdminMovieUpsertDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var movie = await _moviesAdminService.UpdateMovieAsync(movieId, request, cancellationToken);
            return movie is null ? NotFound() : Ok(movie);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Unable to update movie.",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    [HttpDelete("movies/{movieId:guid}")]
    public async Task<IActionResult> DeleteMovie(Guid movieId, CancellationToken cancellationToken = default)
        => await _moviesAdminService.DeleteMovieAsync(movieId, cancellationToken) ? NoContent() : NotFound();

    [HttpGet("showtimes")]
    public async Task<ActionResult<IReadOnlyList<MoviesAdminShowtimeListItemDto>>> GetShowtimes(
        [FromQuery] Guid? movieId,
        [FromQuery] DateOnly? businessDate,
        CancellationToken cancellationToken = default)
        => Ok(await _moviesAdminService.GetShowtimesAsync(movieId, businessDate, cancellationToken));

    [HttpPost("showtimes")]
    public async Task<ActionResult<MoviesAdminShowtimeListItemDto>> CreateShowtime(
        [FromBody] MoviesAdminShowtimeUpsertDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await _moviesAdminService.CreateShowtimeAsync(request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Unable to create showtime.",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    [HttpPut("showtimes/{showtimeId:guid}")]
    public async Task<ActionResult<MoviesAdminShowtimeListItemDto>> UpdateShowtime(
        Guid showtimeId,
        [FromBody] MoviesAdminShowtimeUpsertDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var showtime = await _moviesAdminService.UpdateShowtimeAsync(showtimeId, request, cancellationToken);
            return showtime is null ? NotFound() : Ok(showtime);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Unable to update showtime.",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    [HttpDelete("showtimes/{showtimeId:guid}")]
    public async Task<IActionResult> DeleteShowtime(Guid showtimeId, CancellationToken cancellationToken = default)
        => await _moviesAdminService.DeleteShowtimeAsync(showtimeId, cancellationToken) ? NoContent() : NotFound();
}

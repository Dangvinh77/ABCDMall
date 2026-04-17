using ABCDMall.Modules.Movies.Application.DTOs.Movies;
using ABCDMall.Modules.Movies.Application.DTOs.Showtimes;
using ABCDMall.Modules.Movies.Application.Services.Movies;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers;

[ApiController]
[Route("api/movies")]
public sealed class MoviesController : ControllerBase
{
    private readonly IMovieQueryService _movieQueryService;
    private readonly IValidator<MovieListQueryDto> _movieListQueryValidator;
    private readonly IValidator<MovieShowtimesQueryDto> _movieShowtimesQueryValidator;

    public MoviesController(
        IMovieQueryService movieQueryService,
        IValidator<MovieListQueryDto> movieListQueryValidator,
        IValidator<MovieShowtimesQueryDto> movieShowtimesQueryValidator)
    {
        _movieQueryService = movieQueryService;
        _movieListQueryValidator = movieListQueryValidator;
        _movieShowtimesQueryValidator = movieShowtimesQueryValidator;
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
        var query = new MovieListQueryDto
        {
            Status = status
        };

        var validationResult = await _movieListQueryValidator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(ToValidationProblemDetails(validationResult));
        }

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
        var query = new MovieShowtimesQueryDto
        {
            BusinessDate = businessDate
        };

        var validationResult = await _movieShowtimesQueryValidator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(ToValidationProblemDetails(validationResult));
        }

        var response = await _movieQueryService.GetShowtimesByMovieIdAsync(movieId, businessDate, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    private static ValidationProblemDetails ToValidationProblemDetails(FluentValidation.Results.ValidationResult validationResult)
    {
        return new ValidationProblemDetails(
            validationResult.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(x => x.ErrorMessage).ToArray()));
    }
}

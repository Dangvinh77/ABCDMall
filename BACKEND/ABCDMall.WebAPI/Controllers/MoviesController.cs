using ABCDMall.Modules.Movies.Application.DTOs.Feedbacks;
using ABCDMall.Modules.Movies.Application.DTOs.Movies;
using ABCDMall.Modules.Movies.Application.DTOs.Showtimes;
using ABCDMall.Modules.Movies.Application.Services.Feedbacks;
using ABCDMall.Modules.Movies.Application.Services.Movies;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers;

[ApiController]
[Route("api/movies")]
public sealed class MoviesController : ControllerBase
{
    private readonly IMovieQueryService _movieQueryService;
    private readonly IMovieFeedbackService _movieFeedbackService;
    private readonly IValidator<MovieListQueryDto> _movieListQueryValidator;
    private readonly IValidator<MovieShowtimesQueryDto> _movieShowtimesQueryValidator;
    private readonly IValidator<CreateMovieFeedbackRequestDto> _createFeedbackValidator;

    public MoviesController(
        IMovieQueryService movieQueryService,
        IMovieFeedbackService movieFeedbackService,
        IValidator<MovieListQueryDto> movieListQueryValidator,
        IValidator<MovieShowtimesQueryDto> movieShowtimesQueryValidator,
        IValidator<CreateMovieFeedbackRequestDto> createFeedbackValidator)
    {
        _movieQueryService = movieQueryService;
        _movieFeedbackService = movieFeedbackService;
        _movieListQueryValidator = movieListQueryValidator;
        _movieShowtimesQueryValidator = movieShowtimesQueryValidator;
        _createFeedbackValidator = createFeedbackValidator;
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

    [HttpGet("{movieId:guid}/feedbacks")]
    public async Task<ActionResult<MovieFeedbackListResponseDto>> GetMovieFeedbacks(
        Guid movieId,
        [FromQuery] int? rating,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _movieFeedbackService.GetByMovieAsync(
                movieId,
                rating,
                page,
                pageSize,
                cancellationToken);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Unable to load movie feedback.",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    [HttpPost("{movieId:guid}/feedbacks")]
    public async Task<ActionResult<MovieFeedbackResponseDto>> CreateMovieFeedback(
        Guid movieId,
        [FromBody] CreateMovieFeedbackRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _createFeedbackValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(ToValidationProblemDetails(validationResult));
        }

        try
        {
            var response = await _movieFeedbackService.CreateForMovieAsync(movieId, request, cancellationToken);
            return CreatedAtAction(nameof(GetMovieFeedbacks), new { movieId }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Unable to create movie feedback.",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
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

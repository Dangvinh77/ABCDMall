using ABCDMall.Modules.Movies.Application.DTOs.Showtimes;
using ABCDMall.Modules.Movies.Application.Services.Showtimes;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers;

[ApiController]
[Route("api/showtimes")]
public sealed class ShowtimesController : ControllerBase
{
    private readonly ISeatMapQueryService _seatMapQueryService;
    private readonly IShowtimeQueryService _showtimeQueryService;
    private readonly IValidator<ShowtimeListQueryDto> _showtimeListQueryValidator;

    public ShowtimesController(
        IShowtimeQueryService showtimeQueryService,
        ISeatMapQueryService seatMapQueryService,
        IValidator<ShowtimeListQueryDto> showtimeListQueryValidator)
    {
        _showtimeQueryService = showtimeQueryService;
        _seatMapQueryService = seatMapQueryService;
        _showtimeListQueryValidator = showtimeListQueryValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ShowtimeListItemDto>>> GetShowtimes(
        [FromQuery] Guid? movieId,
        [FromQuery] Guid? cinemaId,
        [FromQuery] DateOnly? businessDate,
        [FromQuery] string? hallType,
        [FromQuery] string? language,
        CancellationToken cancellationToken = default)
    {
        var query = new ShowtimeListQueryDto
        {
            MovieId = movieId,
            CinemaId = cinemaId,
            BusinessDate = businessDate,
            HallType = hallType,
            Language = language
        };

        var validationResult = await _showtimeListQueryValidator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(ToValidationProblemDetails(validationResult));
        }

        var response = await _showtimeQueryService.GetListAsync(
            movieId,
            cinemaId,
            businessDate,
            hallType,
            language,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("{showtimeId:guid}")]
    public async Task<ActionResult<ShowtimeDetailResponseDto>> GetShowtimeById(
        Guid showtimeId,
        CancellationToken cancellationToken = default)
    {
        var response = await _showtimeQueryService.GetByIdAsync(showtimeId, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpGet("{showtimeId:guid}/seat-map")]
    public async Task<ActionResult<SeatMapResponseDto>> GetSeatMap(
        Guid showtimeId,
        CancellationToken cancellationToken = default)
    {
        var response = await _seatMapQueryService.GetByShowtimeIdAsync(showtimeId, cancellationToken);
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

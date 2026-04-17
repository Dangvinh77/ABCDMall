using ABCDMall.Modules.Movies.Application.DTOs.Bookings;
using ABCDMall.Modules.Movies.Application.Services.Bookings;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers;

[ApiController]
[Route("api/bookings")]
public sealed class BookingsController : ControllerBase
{
    private readonly IBookingQuoteService _bookingQuoteService;
    private readonly IValidator<BookingQuoteRequestDto> _bookingQuoteValidator;
    private readonly IBookingHoldService _bookingHoldService;
    private readonly IValidator<CreateBookingHoldRequestDto> _bookingHoldValidator;

    public BookingsController(
        IBookingQuoteService bookingQuoteService,
        IValidator<BookingQuoteRequestDto> bookingQuoteValidator,
        IBookingHoldService bookingHoldService,
        IValidator<CreateBookingHoldRequestDto> bookingHoldValidator)
    {
        _bookingQuoteService = bookingQuoteService;
        _bookingQuoteValidator = bookingQuoteValidator;
        _bookingHoldService = bookingHoldService;
        _bookingHoldValidator = bookingHoldValidator;
    }

    [HttpPost("quote")]
    public async Task<ActionResult<BookingQuoteResponseDto>> Quote(
        [FromBody] BookingQuoteRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _bookingQuoteValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(ToValidationProblemDetails(validationResult));
        }

        try
        {
            var result = await _bookingQuoteService.QuoteAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Unable to create booking quote.",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    [HttpPost("holds")]
    public async Task<ActionResult<BookingHoldResponseDto>> CreateHold(
        [FromBody] CreateBookingHoldRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _bookingHoldValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(ToValidationProblemDetails(validationResult));
        }

        try
        {
            var result = await _bookingHoldService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetHold), new { holdId = result.HoldId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Unable to create booking hold.",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    [HttpGet("holds/{holdId:guid}")]
    public async Task<ActionResult<BookingHoldResponseDto>> GetHold(
        Guid holdId,
        CancellationToken cancellationToken = default)
    {
        var result = await _bookingHoldService.GetByIdAsync(holdId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("holds/{holdId:guid}")]
    public async Task<IActionResult> ReleaseHold(
        Guid holdId,
        CancellationToken cancellationToken = default)
    {
        var released = await _bookingHoldService.ReleaseAsync(holdId, cancellationToken);
        return released ? NoContent() : NotFound();
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

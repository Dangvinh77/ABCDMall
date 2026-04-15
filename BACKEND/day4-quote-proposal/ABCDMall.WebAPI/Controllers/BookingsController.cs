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

    public BookingsController(
        IBookingQuoteService bookingQuoteService,
        IValidator<BookingQuoteRequestDto> bookingQuoteValidator)
    {
        _bookingQuoteService = bookingQuoteService;
        _bookingQuoteValidator = bookingQuoteValidator;
    }

    [HttpPost("quote")]
    public async Task<ActionResult<BookingQuoteResponseDto>> Quote(
        [FromBody] BookingQuoteRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _bookingQuoteValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(
                validationResult.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(x => x.ErrorMessage).ToArray())));
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
}

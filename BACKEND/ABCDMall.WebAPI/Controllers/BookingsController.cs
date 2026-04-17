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

    [HttpPost("hold")]
    public async Task<ActionResult<BookingHoldResponseDto>> CreateHold(
        [FromBody] CreateBookingHoldRequestDto request,
        CancellationToken cancellationToken = default
        )
    {
        //kiểm tra dữ liệu đầu vào
        var validationResult = await _bookingHoldValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(ToValidationProblemDetails(validationResult));
        }

        try
        {
            var result = await _bookingHoldService.CreateAsync(request, cancellationToken);//gọi service để tạo booking hold
            return CreatedAtAction(nameof(GetHold), new { holdId = result.HoldId }, result);//trả về kết quả với mã 201 Created và đường dẫn để lấy thông tin chi tiết của booking hold vừa tạo
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
        //hàm này lấy thông tin chi tiết của một booking hold dựa trên holdId. Nếu không tìm thấy sẽ trả về NotFound, nếu tìm thấy sẽ trả về thông tin chi tiết của booking hold đó.
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

    [HttpPost("holds/{holdId:guid}/confirm")]
    public async Task<ActionResult<BookingHoldResponseDto>> ConfirmHold(
        Guid holdId,
        CancellationToken cancellationToken = default)
    {
        // DAY5 TEST-ONLY CONFIRM FLOW:
        // Endpoint này chỉ dùng để test luồng hold -> booked sau khi bấm Confirm trên FE.
        // Khi code booking/payment hoàn chỉnh, thay endpoint này bằng use case tạo Booking/Payment thật.
        try
        {
            var result = await _bookingHoldService.ConfirmAsync(holdId, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Unable to confirm booking hold.",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    private static ValidationProblemDetails ToValidationProblemDetails(FluentValidation.Results.ValidationResult validationResult)
    {
        //hàm này chuyển kết quả validate từ FluentValidation sang định dạng ValidationProblemDetails của ASP.NET Core để trả về lỗi chi tiết cho client
        return new ValidationProblemDetails(
            validationResult.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(x => x.ErrorMessage).ToArray()));
    }
}

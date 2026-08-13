using ABCDMall.Modules.Events.Application.DTOs;
using ABCDMall.Modules.Events.Application.DTOs.Events;
using ABCDMall.Modules.Events.Application.Services.Events;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EventsController : ControllerBase
{
    private readonly IEventQueryService _eventQueryService;
    private readonly IEventCommandService _eventCommandService;
    private readonly IValidator<EventListQueryDto> _listQueryValidator;
    private readonly IValidator<CreateEventRequestDto> _createValidator;
    private readonly IValidator<UpdateEventRequestDto> _updateValidator;

    public EventsController(
        IEventQueryService eventQueryService,
        IEventCommandService eventCommandService,
        IValidator<EventListQueryDto> listQueryValidator,
        IValidator<CreateEventRequestDto> createValidator,
        IValidator<UpdateEventRequestDto> updateValidator)
    {
        _eventQueryService   = eventQueryService;
        _eventCommandService = eventCommandService;
        _listQueryValidator  = listQueryValidator;
        _createValidator     = createValidator;
        _updateValidator     = updateValidator;
    }

    /// <summary>
    /// Lấy danh sách sự kiện. Hỗ trợ filter: keyword, eventType (1/2), status (upcoming/ongoing/ended), isHot.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<EventDto>>> GetEvents(
        [FromQuery] string? keyword,
        [FromQuery] int? eventType,
        [FromQuery] string? status,
        [FromQuery] bool? isHot,
        CancellationToken cancellationToken = default)
    {
        var query = new EventListQueryDto
        {
            Keyword   = keyword,
            EventType = eventType,
            Status    = status,
            IsHot     = isHot
        };

        var validationResult = await _listQueryValidator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(ToValidationProblemDetails(validationResult));
        }

        return Ok(await _eventQueryService.GetListAsync(query, cancellationToken));
    }

    /// <summary>
    /// Lấy danh sách sự kiện HOT cho Banner Slider trang chủ.
    /// </summary>
    [HttpGet("hot")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<EventDto>>> GetHotEvents(
        CancellationToken cancellationToken = default)
    {
        return Ok(await _eventQueryService.GetHotEventsAsync(cancellationToken));
    }

    /// <summary>
    /// Lấy chi tiết một sự kiện theo Id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<EventDto>> GetEventById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var ev = await _eventQueryService.GetByIdAsync(id, cancellationToken);
        return ev is null ? NotFound() : Ok(ev);
    }

    /// <summary>
    /// Tạo sự kiện mới. Yêu cầu role Admin hoặc Manager.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateEvent(
        [FromBody] CreateEventRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(ToValidationProblemDetails(validationResult));
        }

        try
        {
            var newId = await _eventCommandService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(
                nameof(GetEventById),
                new { id = newId },
                new { id = newId, message = "Event created successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cập nhật sự kiện. Yêu cầu role Admin hoặc Manager.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateEvent(
        Guid id,
        [FromBody] UpdateEventRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(ToValidationProblemDetails(validationResult));
        }

        try
        {
            var updated = await _eventCommandService.UpdateAsync(id, request, cancellationToken);
            if (!updated) return NotFound();
            return Ok(new { message = "Event updated successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Xóa sự kiện. Yêu cầu role Admin hoặc Manager.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteEvent(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var deleted = await _eventCommandService.DeleteAsync(id, cancellationToken);
        if (!deleted) return NotFound();
        return Ok(new { message = "Event deleted successfully." });
    }

    private static ValidationProblemDetails ToValidationProblemDetails(
        FluentValidation.Results.ValidationResult validationResult)
    {
        return new ValidationProblemDetails(
            validationResult.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(x => x.ErrorMessage).ToArray()));
    }
}
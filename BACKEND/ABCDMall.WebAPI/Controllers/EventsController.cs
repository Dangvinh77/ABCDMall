using ABCDMall.Modules.Events.Application.DTOs;
using ABCDMall.Modules.Events.Application.Common;
using ABCDMall.Modules.Events.Application.DTOs.Events;
using ABCDMall.Modules.Events.Application.Services.Events;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
    /// Legacy public list endpoint kept for backward compatibility.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<EventDto>>> GetEvents(
        [FromQuery] string? keyword,
        [FromQuery] string? timeFilter,
        [FromQuery] string? shopId,
        [FromQuery] int? approvalStatus,
        [FromQuery] bool includeAllStatuses = false,
        CancellationToken cancellationToken = default)
    {
        var query = new EventListQueryDto
        {
            Keyword = keyword,
            TimeFilter = timeFilter,
            ShopId = shopId,
            ApprovalStatus = approvalStatus,
            IncludeAllStatuses = includeAllStatuses
        };

        var validationResult = await _listQueryValidator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(ToValidationProblemDetails(validationResult));
        }

        return Ok(await _eventQueryService.GetListAsync(query, cancellationToken));
    }

    /// <summary>
    /// Legacy "hot" endpoint mapped to active public events.
    /// </summary>
    [HttpGet("hot")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<EventDto>>> GetHotEvents(
        CancellationToken cancellationToken = default)
    {
        return Ok(await _eventQueryService.GetActiveEventsAsync(cancellationToken));
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
    /// Legacy create endpoint routed to admin/manager flows.
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
            ApplicationResult<Guid> createResult;

            if (User.IsInRole("Manager"))
            {
                var shopId = User.FindFirstValue("shopId");
                if (string.IsNullOrWhiteSpace(shopId))
                {
                    return BadRequest(new { message = "Manager shop id is missing." });
                }

                createResult = await _eventCommandService.CreateShopEventAsync(shopId, request, cancellationToken);
            }
            else
            {
                createResult = await _eventCommandService.CreateMallEventAsync(request, cancellationToken);
            }

            if (createResult.Status != ApplicationResultStatus.Ok)
            {
                return FromResult(createResult);
            }

            var newId = createResult.Value;
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
            return FromResult(await _eventCommandService.UpdateAsync(id, request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete is not supported in the current event workflow.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteEvent(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return StatusCode(StatusCodes.Status501NotImplemented, new { message = $"Deleting event '{id}' is not supported by the current workflow." });
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

    private IActionResult FromResult<T>(ApplicationResult<T> result) => result.Status switch
    {
        ApplicationResultStatus.Ok => Ok(result.Value),
        ApplicationResultStatus.NotFound => NotFound(new { message = result.Error }),
        ApplicationResultStatus.BadRequest => BadRequest(new { message = result.Error }),
        _ => StatusCode(StatusCodes.Status500InternalServerError, new { message = "Unexpected error." })
    };
}

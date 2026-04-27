using ABCDMall.Modules.Events.Application.Common;
using ABCDMall.Modules.Events.Application.DTOs;
using ABCDMall.Modules.Events.Application.DTOs.Events;
using ABCDMall.Modules.Events.Application.Services.Events;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/events")]
public sealed class AdminEventsController : ControllerBase
{
    private readonly IEventQueryService _queryService;
    private readonly IEventCommandService _commandService;
    private readonly IValidator<CreateEventRequestDto> _createValidator;

    public AdminEventsController(IEventQueryService queryService, IEventCommandService commandService, IValidator<CreateEventRequestDto> createValidator)
    {
        _queryService = queryService;
        _commandService = commandService;
        _createValidator = createValidator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateMallEvent([FromBody] CreateEventRequestDto request, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(validation.Errors);
        }

        return FromResult(await _commandService.CreateMallEventAsync(request, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EventDto>>> GetAdminReviewEvents(CancellationToken cancellationToken = default)
        => Ok(await _queryService.GetAdminReviewListAsync(cancellationToken));

    [HttpGet("by-status/{status}")]
    public async Task<ActionResult<IReadOnlyList<EventDto>>> GetEventsByStatus(string status, CancellationToken cancellationToken = default)
        => Ok(await _queryService.GetEventsByStatusAsync(status, cancellationToken));

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken = default)
        => FromResult(await _commandService.ApproveAsync(id, cancellationToken));

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectEventRequestDto request, CancellationToken cancellationToken = default)
        => FromResult(await _commandService.RejectAsync(id, request.Reason, cancellationToken));

    private IActionResult FromResult<T>(ApplicationResult<T> result) => result.Status switch
    {
        ApplicationResultStatus.Ok => Ok(result.Value),
        ApplicationResultStatus.NotFound => NotFound(new { message = result.Error }),
        ApplicationResultStatus.BadRequest => BadRequest(new { message = result.Error }),
        _ => StatusCode(StatusCodes.Status500InternalServerError, new { message = "Unexpected error." })
    };
}

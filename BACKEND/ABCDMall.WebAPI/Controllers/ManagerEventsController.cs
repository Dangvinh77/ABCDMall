using System.Security.Claims;
using ABCDMall.Modules.Events.Application.Common;
using ABCDMall.Modules.Events.Application.DTOs;
using ABCDMall.Modules.Events.Application.DTOs.Events;
using ABCDMall.Modules.Events.Application.Services.Events;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers;

[ApiController]
[Authorize(Roles = "Manager")]
[Route("api/manager/events")]
public sealed class ManagerEventsController : ControllerBase
{
    private readonly IEventQueryService _queryService;
    private readonly IEventCommandService _commandService;
    private readonly IValidator<CreateEventRequestDto> _createValidator;

    public ManagerEventsController(IEventQueryService queryService, IEventCommandService commandService, IValidator<CreateEventRequestDto> createValidator)
    {
        _queryService = queryService;
        _commandService = commandService;
        _createValidator = createValidator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateShopEvent([FromBody] CreateEventRequestDto request, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(validation.Errors);
        }

        var shopId = User.FindFirstValue("shopId");
        if (string.IsNullOrWhiteSpace(shopId))
        {
            return BadRequest(new { message = "Manager shop id is missing." });
        }

        return FromResult(await _commandService.CreateShopEventAsync(shopId, request, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EventDto>>> GetMyEvents(CancellationToken cancellationToken = default)
    {
        var shopId = User.FindFirstValue("shopId");
        if (string.IsNullOrWhiteSpace(shopId))
        {
            return BadRequest(new { message = "Manager shop id is missing." });
        }

        return Ok(await _queryService.GetManagerEventsAsync(shopId, cancellationToken));
    }

    [HttpGet("schedule")]
    public async Task<ActionResult<IReadOnlyList<EventDto>>> GetSchedule(CancellationToken cancellationToken = default)
        => Ok(await _queryService.GetManagerScheduleAsync(cancellationToken));

    private IActionResult FromResult<T>(ApplicationResult<T> result) => result.Status switch
    {
        ApplicationResultStatus.Ok => Ok(result.Value),
        ApplicationResultStatus.NotFound => NotFound(new { message = result.Error }),
        ApplicationResultStatus.BadRequest => BadRequest(new { message = result.Error }),
        _ => StatusCode(StatusCodes.Status500InternalServerError, new { message = "Unexpected error." })
    };
}

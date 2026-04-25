using ABCDMall.Modules.Events.Application.Common;
using ABCDMall.Modules.Events.Application.DTOs;
using ABCDMall.Modules.Events.Application.DTOs.Events;
using ABCDMall.Modules.Events.Application.Services.Events;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/public/events")]
public sealed class PublicEventsController : ControllerBase
{
    private readonly IEventQueryService _queryService;
    private readonly IEventCommandService _commandService;
    private readonly IValidator<RegisterEventRequestDto> _registerValidator;

    public PublicEventsController(IEventQueryService queryService, IEventCommandService commandService, IValidator<RegisterEventRequestDto> registerValidator)
    {
        _queryService = queryService;
        _commandService = commandService;
        _registerValidator = registerValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EventDto>>> GetPublicEvents([FromQuery] string? filter, CancellationToken cancellationToken = default)
        => Ok(await _queryService.GetPublicEventsAsync(filter, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EventDto>> GetDetail(Guid id, CancellationToken cancellationToken = default)
    {
        var data = await _queryService.GetByIdAsync(id, cancellationToken);
        return data is null ? NotFound() : Ok(data);
    }

    [HttpGet("shop/{shopId}")]
    public async Task<ActionResult<IReadOnlyList<EventDto>>> GetShopEvents(string shopId, CancellationToken cancellationToken = default)
        => Ok(await _queryService.GetPublicShopEventsAsync(shopId, cancellationToken));

    [HttpGet("active")]
    public async Task<ActionResult<IReadOnlyList<EventDto>>> GetActiveEvents(CancellationToken cancellationToken = default)
        => Ok(await _queryService.GetActiveEventsAsync(cancellationToken));

    [HttpPost("{id:guid}/register")]
    public async Task<IActionResult> Register(Guid id, [FromBody] RegisterEventRequestDto request, CancellationToken cancellationToken = default)
    {
        var validation = await _registerValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(validation.Errors);
        }

        return FromResult(await _commandService.RegisterAsync(id, request, cancellationToken));
    }

    private IActionResult FromResult<T>(ApplicationResult<T> result) => result.Status switch
    {
        ApplicationResultStatus.Ok => Ok(result.Value),
        ApplicationResultStatus.NotFound => NotFound(new { message = result.Error }),
        ApplicationResultStatus.BadRequest => BadRequest(new { message = result.Error }),
        _ => StatusCode(StatusCodes.Status500InternalServerError, new { message = "Unexpected error." })
    };
}

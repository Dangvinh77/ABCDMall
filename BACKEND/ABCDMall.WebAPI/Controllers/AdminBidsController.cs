using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Application.DTOs.Bidding;
using ABCDMall.Modules.Users.Application.Services.Bidding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/bids")]
public sealed class AdminBidsController : ControllerBase
{
    private readonly IBiddingAdminService _biddingAdminService;

    public AdminBidsController(IBiddingAdminService biddingAdminService)
    {
        _biddingAdminService = biddingAdminService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminCarouselBidListItemDto>>> GetUpcomingWeekBids(CancellationToken cancellationToken = default)
        => Ok(await _biddingAdminService.GetUpcomingWeekBidsAsync(cancellationToken));

    [HttpPost("movie-ad")]
    public async Task<IActionResult> UpsertMovieAd(
        [FromBody] CreateOrUpdateMovieCarouselAdRequestDto request,
        CancellationToken cancellationToken = default)
        => FromResult(await _biddingAdminService.UpsertUpcomingWeekMovieAdAsync(request, cancellationToken));

    [HttpPost("trigger-saturday-resolution")]
    public async Task<IActionResult> TriggerSaturdayResolution(CancellationToken cancellationToken = default)
        => FromResult(await _biddingAdminService.ResolveUpcomingWeekBidsAsync(cancellationToken));

    [HttpPost("trigger-monday-publish")]
    public async Task<IActionResult> TriggerMondayPublish(CancellationToken cancellationToken = default)
        => FromResult(await _biddingAdminService.PublishUpcomingWeekCarouselAsync(cancellationToken));

    private IActionResult FromResult<T>(ApplicationResult<T> result)
    {
        return result.Status switch
        {
            ApplicationResultStatus.Ok => Ok(result.Value),
            ApplicationResultStatus.BadRequest => BadRequest(CreateProblemDetails(
                "Admin bidding request is invalid.",
                result.Error,
                StatusCodes.Status400BadRequest)),
            ApplicationResultStatus.NotFound => NotFound(CreateProblemDetails(
                "Requested bidding resource was not found.",
                result.Error,
                StatusCodes.Status404NotFound)),
            ApplicationResultStatus.Unauthorized => Unauthorized(CreateProblemDetails(
                "You are not allowed to perform this bidding action.",
                result.Error,
                StatusCodes.Status401Unauthorized)),
            _ => StatusCode(StatusCodes.Status500InternalServerError, CreateProblemDetails(
                "Admin bidding request failed unexpectedly.",
                "An unexpected error occurred.",
                StatusCodes.Status500InternalServerError))
        };
    }

    private static ProblemDetails CreateProblemDetails(string title, string? detail, int status)
        => new()
        {
            Title = title,
            Detail = detail,
            Status = status
        };
}

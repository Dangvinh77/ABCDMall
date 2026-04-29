using System.Security.Claims;
using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Application.DTOs.Bidding;
using ABCDMall.Modules.Users.Application.Services.Bidding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace ABCDMall.WebAPI.Controllers;

[ApiController]
[Authorize(Roles = "Manager")]
[Route("api/manager/bids")]
public sealed class ManagerBidsController : ControllerBase
{
    private readonly IBiddingManagerService _biddingManagerService;

    public ManagerBidsController(IBiddingManagerService biddingManagerService)
    {
        _biddingManagerService = biddingManagerService;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(15_000_000)]
    public async Task<IActionResult> SubmitBid(
        [FromForm] SubmitCarouselBidRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var shopId = GetOwnerShopId();
            var result = await _biddingManagerService.SubmitBidAsync(shopId ?? string.Empty, request, cancellationToken);
            return FromResult(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(CreateProblemDetails("Unable to submit bid.", ex.Message, StatusCodes.Status400BadRequest));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, CreateProblemDetails(
                "Bid submission failed unexpectedly.",
                ex.Message,
                StatusCodes.Status500InternalServerError));
        }
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ManagerCarouselBidDto>>> GetMyBids(CancellationToken cancellationToken = default)
    {
        var shopId = GetOwnerShopId();
        if (string.IsNullOrWhiteSpace(shopId))
        {
            return BadRequest(CreateProblemDetails(
                "Manager shop id is missing.",
                "Manager account does not have a shop id.",
                StatusCodes.Status400BadRequest));
        }

        return Ok(await _biddingManagerService.GetBidHistoryAsync(shopId, cancellationToken));
    }

    [HttpPost("{id}/pay")]
    public async Task<IActionResult> PayBid(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var shopId = GetOwnerShopId();
            var result = await _biddingManagerService.CreatePaymentCheckoutSessionAsync(shopId ?? string.Empty, id, cancellationToken);
            return FromResult(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(CreateProblemDetails(
                "Unable to create bid payment checkout session.",
                ex.Message,
                StatusCodes.Status400BadRequest));
        }
        catch (StripeException ex)
        {
            return BadRequest(CreateProblemDetails(
                "Bid payment checkout session failed.",
                ex.StripeError?.Message ?? ex.Message,
                StatusCodes.Status400BadRequest));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, CreateProblemDetails(
                "Bid payment failed unexpectedly.",
                ex.Message,
                StatusCodes.Status500InternalServerError));
        }
    }

    private IActionResult FromResult<T>(ApplicationResult<T> result)
    {
        return result.Status switch
        {
            ApplicationResultStatus.Ok => Ok(result.Value),
            ApplicationResultStatus.BadRequest => BadRequest(CreateProblemDetails(
                "Bidding request is invalid.",
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
                "Bidding request failed unexpectedly.",
                "An unexpected error occurred.",
                StatusCodes.Status500InternalServerError))
        };
    }

    private string? GetOwnerShopId()
        => User.FindFirstValue("shopId");

    private static ProblemDetails CreateProblemDetails(string title, string? detail, int status)
        => new()
        {
            Title = title,
            Detail = detail,
            Status = status
        };
}

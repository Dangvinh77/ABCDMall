using System.Security.Claims;
using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Application.DTOs.RentalPayments;
using ABCDMall.Modules.Users.Application.Services.RentalPayments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace ABCDMall.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class RentalPaymentsController : ControllerBase
{
    private readonly IRentalPaymentService _rentalPaymentService;
    private readonly ILogger<RentalPaymentsController> _logger;

    public RentalPaymentsController(
        IRentalPaymentService rentalPaymentService,
        ILogger<RentalPaymentsController> logger)
    {
        _rentalPaymentService = rentalPaymentService;
        _logger = logger;
    }

    [HttpPost("{billId}/checkout-session")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> CreateCheckoutSession(string billId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        var shopId = User.FindFirstValue("shopId");

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized("User id is missing from token.");
        }

        var result = await _rentalPaymentService.CreateCheckoutSessionAsync(
            billId,
            userId,
            shopId,
            cancellationToken);

        return result.Status switch
        {
            ApplicationResultStatus.Ok => Ok(result.Value),
            ApplicationResultStatus.BadRequest => BadRequest(result.Error),
            ApplicationResultStatus.NotFound => NotFound(result.Error),
            ApplicationResultStatus.Unauthorized => Unauthorized(result.Error),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpPost("webhooks/stripe")]
    [AllowAnonymous]
    public async Task<IActionResult> StripeWebhook(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        var signatureHeader = Request.Headers["Stripe-Signature"].ToString();

        if (string.IsNullOrWhiteSpace(signatureHeader))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Stripe signature header is missing."
            });
        }

        try
        {
            await _rentalPaymentService.ProcessStripeWebhookAsync(payload, signatureHeader, cancellationToken);
            return Ok();
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Rental Stripe webhook signature or payload validation failed.");
            return BadRequest(new ProblemDetails
            {
                Title = "Stripe webhook signature validation failed.",
                Detail = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Rental Stripe webhook business validation failed.");
            return BadRequest(new ProblemDetails
            {
                Title = "Rental Stripe webhook could not be processed.",
                Detail = ex.Message
            });
        }
    }
}

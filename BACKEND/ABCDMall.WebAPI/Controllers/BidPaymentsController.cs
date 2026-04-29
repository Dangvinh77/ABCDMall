using System.Text;
using ABCDMall.Modules.Users.Application.Services.Bidding;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace ABCDMall.WebAPI.Controllers;

[ApiController]
[Route("api/bids/payments")]
public sealed class BidPaymentsController : ControllerBase
{
    private const string StripeWebhookLogRelativePath = "logs/bid-stripe-webhook-errors.log";
    private readonly IBidPaymentService _bidPaymentService;
    private readonly ILogger<BidPaymentsController> _logger;

    public BidPaymentsController(
        IBidPaymentService bidPaymentService,
        ILogger<BidPaymentsController> logger)
    {
        _bidPaymentService = bidPaymentService;
        _logger = logger;
    }

    [HttpPost("webhooks/stripe")]
    public async Task<IActionResult> StripeWebhook(CancellationToken cancellationToken = default)
    {
        string payload;
        using (var reader = new StreamReader(Request.Body))
        {
            payload = await reader.ReadToEndAsync(cancellationToken);
        }

        var signatureHeader = Request.Headers["Stripe-Signature"].ToString();
        if (string.IsNullOrWhiteSpace(signatureHeader))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Stripe signature header is missing.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            await _bidPaymentService.ProcessWebhookAsync(payload, signatureHeader, cancellationToken);
            return Ok();
        }
        catch (StripeException ex)
        {
            await WriteStripeWebhookErrorAsync("Bid Stripe signature/payload validation failed.", ex, payload, cancellationToken);
            _logger.LogWarning(ex, "Bid Stripe webhook signature or payload validation failed.");

            return BadRequest(new ProblemDetails
            {
                Title = "Stripe webhook signature validation failed.",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (InvalidOperationException ex)
        {
            await WriteStripeWebhookErrorAsync("Bid Stripe webhook business validation failed.", ex, payload, cancellationToken);
            _logger.LogWarning(ex, "Bid Stripe webhook business validation failed.");

            return BadRequest(new ProblemDetails
            {
                Title = "Bid payment webhook could not be processed.",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            await WriteStripeWebhookErrorAsync("Bid Stripe webhook failed unexpectedly.", ex, payload, cancellationToken);
            _logger.LogError(ex, "Bid Stripe webhook failed unexpectedly.");

            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Bid payment webhook failed unexpectedly.",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    private static async Task WriteStripeWebhookErrorAsync(
        string title,
        Exception exception,
        string payload,
        CancellationToken cancellationToken)
    {
        var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(logDirectory);

        var logPath = Path.Combine(AppContext.BaseDirectory, StripeWebhookLogRelativePath);
        var builder = new StringBuilder()
            .AppendLine($"[{DateTime.UtcNow:O}] {title}")
            .AppendLine(exception.ToString())
            .AppendLine("Payload:")
            .AppendLine(payload)
            .AppendLine(new string('-', 80));

        await System.IO.File.AppendAllTextAsync(logPath, builder.ToString(), cancellationToken);
    }
}

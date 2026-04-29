using System.IO;
using System.Text;
using ABCDMall.Modules.Movies.Application.DTOs.Payments;
using ABCDMall.Modules.Movies.Application.Services.Bookings;
using ABCDMall.Modules.Movies.Application.Services.Payments;
using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Application.DTOs.RentalPayments;
using ABCDMall.Modules.Users.Application.Services.RentalPayments;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Stripe;

namespace ABCDMall.WebAPI.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController : ControllerBase
{
    private const string StripeWebhookLogRelativePath = "logs/stripe-webhook-errors.log";
    private readonly IPaymentService _paymentService;
    private readonly IStripePaymentService _stripePaymentService;
    private readonly IRentalPaymentService _rentalPaymentService;
    private readonly IValidator<CreateStripeCheckoutSessionRequestDto> _createStripeCheckoutSessionValidator;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentService paymentService,
        IStripePaymentService stripePaymentService,
        IRentalPaymentService rentalPaymentService,
        IValidator<CreateStripeCheckoutSessionRequestDto> createStripeCheckoutSessionValidator,
        ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _stripePaymentService = stripePaymentService;
        _rentalPaymentService = rentalPaymentService;
        _createStripeCheckoutSessionValidator = createStripeCheckoutSessionValidator;
        _logger = logger;
    }

    [HttpGet("{paymentId:guid}")]
    public async Task<ActionResult<PaymentStatusResponseDto>> GetPaymentStatus(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        var result = await _paymentService.GetStatusAsync(paymentId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("checkout-session/stripe/rental-bills")]
    [Authorize(Roles = "Manager")]
    public async Task<ActionResult<RentalCheckoutSessionResponseDto>> CreateRentalStripeCheckoutSession(
        [FromBody] CreateRentalCheckoutSessionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.BillId))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Bill id is required.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        var shopId = User.FindFirstValue("shopId");

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "User id is missing from token.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        try
        {
            var result = await _rentalPaymentService.CreateCheckoutSessionAsync(
                request.BillId,
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
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Rental Stripe checkout session creation failed for bill {BillId}.", request.BillId);

            return BadRequest(new ProblemDetails
            {
                Title = "Stripe checkout session creation failed.",
                Detail = ex.StripeError?.Message ?? ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Rental Stripe checkout session could not be created for bill {BillId}.", request.BillId);

            return BadRequest(new ProblemDetails
            {
                Title = "Unable to create Stripe checkout session.",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    [HttpPost("checkout-session/stripe")]
    public async Task<ActionResult<StripeCheckoutSessionResponseDto>> CreateStripeCheckoutSession(
        [FromBody] CreateStripeCheckoutSessionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _createStripeCheckoutSessionValidator.ValidateAsync(request, cancellationToken);
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
            var result = await _stripePaymentService.CreateCheckoutSessionAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Unable to create Stripe checkout session.",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Stripe checkout session creation failed for booking {BookingId}.", request.BookingId);

            return BadRequest(new ProblemDetails
            {
                Title = "Stripe checkout session creation failed.",
                Detail = ex.StripeError?.Message ?? ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
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
            await _stripePaymentService.ProcessWebhookAsync(payload, signatureHeader, cancellationToken);
            await _rentalPaymentService.ProcessStripeWebhookAsync(payload, signatureHeader, cancellationToken);
            return Ok();
        }
        catch (StripeException ex)
        {
            await WriteStripeWebhookErrorAsync("Stripe signature/payload validation failed.", ex, payload, cancellationToken);
            _logger.LogWarning(
                ex,
                "Stripe webhook signature or payload validation failed. Signature: {SignatureHeader}",
                signatureHeader);

            return BadRequest(new ProblemDetails
            {
                Title = "Stripe webhook signature validation failed.",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (InvalidOperationException ex)
        {
            await WriteStripeWebhookErrorAsync("Stripe webhook business validation failed.", ex, payload, cancellationToken);
            _logger.LogWarning(ex, "Stripe webhook business validation failed.");

            return BadRequest(new ProblemDetails
            {
                Title = "Stripe webhook could not be processed.",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            await WriteStripeWebhookErrorAsync("Stripe webhook failed unexpectedly.", ex, payload, cancellationToken);
            _logger.LogError(ex, "Stripe webhook failed unexpectedly.");

            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Stripe webhook failed unexpectedly.",
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

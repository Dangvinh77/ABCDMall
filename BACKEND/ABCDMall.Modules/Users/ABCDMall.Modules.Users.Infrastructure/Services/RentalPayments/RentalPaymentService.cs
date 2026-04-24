using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Application.DTOs.RentalPayments;
using ABCDMall.Modules.Users.Application.Services.RentalPayments;
using ABCDMall.Modules.Users.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace ABCDMall.Modules.Users.Infrastructure.Services.RentalPayments;

public sealed class RentalPaymentService : IRentalPaymentService
{
    private const string RentalModuleMetadataValue = "rental";
    private const string PaidStatus = "Paid";
    private const string UnpaidStatus = "Unpaid";

    private static readonly HashSet<string> ZeroDecimalCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "bif", "clp", "djf", "gnf", "jpy", "kmf", "krw", "mga", "pyg", "rwf", "ugx", "vnd", "vuv", "xaf", "xof", "xpf"
    };

    private readonly MallDbContext _context;
    private readonly StripeSettings _settings;

    public RentalPaymentService(MallDbContext context, IOptions<StripeSettings> settings)
    {
        _context = context;
        _settings = settings.Value;
    }

    public async Task<ApplicationResult<RentalCheckoutSessionResponseDto>> CreateCheckoutSessionAsync(
        string billId,
        string managerUserId,
        string? managerShopId,
        CancellationToken cancellationToken = default)
    {
        var bill = await _context.ShopMonthlyBills.FirstOrDefaultAsync(x => x.Id == billId, cancellationToken);
        if (bill is null)
        {
            return ApplicationResult<RentalCheckoutSessionResponseDto>.NotFound("Rental bill does not exist.");
        }

        if (string.IsNullOrWhiteSpace(managerShopId) || bill.ShopInfoId != managerShopId)
        {
            return ApplicationResult<RentalCheckoutSessionResponseDto>.Unauthorized("You can only pay your own rental bills.");
        }

        if (string.Equals(bill.PaymentStatus, PaidStatus, StringComparison.OrdinalIgnoreCase))
        {
            return ApplicationResult<RentalCheckoutSessionResponseDto>.BadRequest("This rental bill is already paid.");
        }

        if (bill.TotalDue <= 0)
        {
            return ApplicationResult<RentalCheckoutSessionResponseDto>.BadRequest("Rental bill total due must be greater than 0.");
        }

        if (string.IsNullOrWhiteSpace(_settings.SecretKey))
        {
            return ApplicationResult<RentalCheckoutSessionResponseDto>.BadRequest("StripeSettings:SecretKey is missing.");
        }

        if (string.IsNullOrWhiteSpace(_settings.FrontendBaseUrl))
        {
            return ApplicationResult<RentalCheckoutSessionResponseDto>.BadRequest("StripeSettings:FrontendBaseUrl is missing.");
        }

        StripeConfiguration.ApiKey = _settings.SecretKey;

        var manager = await _context.Users.FirstOrDefaultAsync(x => x.Id == managerUserId, cancellationToken);
        var frontendBaseUrl = _settings.FrontendBaseUrl.TrimEnd('/');
        var successUrl = $"{frontendBaseUrl}/shop-info?payment=success&billId={Uri.EscapeDataString(bill.Id ?? string.Empty)}&session_id={{CHECKOUT_SESSION_ID}}";
        var cancelUrl = $"{frontendBaseUrl}/shop-info?payment=cancel&billId={Uri.EscapeDataString(bill.Id ?? string.Empty)}";
        var description = $"ABCDMall rental bill {bill.Month} - {bill.ShopName}";

        var metadata = new Dictionary<string, string>
        {
            ["module"] = RentalModuleMetadataValue,
            ["billId"] = bill.Id ?? string.Empty,
            ["shopInfoId"] = bill.ShopInfoId,
            ["managerUserId"] = managerUserId,
            ["billingMonth"] = bill.BillingMonthKey
        };

        var options = new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            CustomerEmail = manager?.Email,
            Metadata = metadata,
            PaymentMethodTypes = new List<string> { "card" },
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                Metadata = metadata,
                Description = description,
                ReceiptEmail = manager?.Email
            },
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "vnd",
                        UnitAmount = ConvertMajorAmountToMinorUnits(bill.TotalDue, "vnd"),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"Rental bill - {bill.ShopName}",
                            Description = description
                        }
                    }
                }
            }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options, cancellationToken: cancellationToken);
        if (string.IsNullOrWhiteSpace(session.Url))
        {
            return ApplicationResult<RentalCheckoutSessionResponseDto>.BadRequest("Stripe Checkout session URL was not returned.");
        }

        bill.StripeSessionId = session.Id;
        bill.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return ApplicationResult<RentalCheckoutSessionResponseDto>.Ok(new RentalCheckoutSessionResponseDto
        {
            BillId = bill.Id ?? string.Empty,
            SessionId = session.Id,
            CheckoutUrl = session.Url,
            PaymentStatus = bill.PaymentStatus
        });
    }

    public async Task ProcessStripeWebhookAsync(
        string payload,
        string signatureHeader,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.SecretKey))
        {
            throw new InvalidOperationException("StripeSettings:SecretKey is missing.");
        }

        if (string.IsNullOrWhiteSpace(_settings.WebhookSecret))
        {
            throw new InvalidOperationException("StripeSettings:WebhookSecret is missing.");
        }

        StripeConfiguration.ApiKey = _settings.SecretKey;

        var stripeEvent = EventUtility.ConstructEvent(payload, signatureHeader, _settings.WebhookSecret);
        if (!string.Equals(stripeEvent.Type, "checkout.session.completed", StringComparison.Ordinal)
            && !string.Equals(stripeEvent.Type, "checkout.session.expired", StringComparison.Ordinal))
        {
            return;
        }

        var session = stripeEvent.Data.Object as Session
            ?? throw new InvalidOperationException("Stripe webhook payload is not a Checkout Session.");

        if (!string.Equals(GetMetadataValue(session.Metadata, "module"), RentalModuleMetadataValue, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var billId = GetMetadataValue(session.Metadata, "billId");
        if (string.IsNullOrWhiteSpace(billId))
        {
            return;
        }

        var bill = await _context.ShopMonthlyBills.FirstOrDefaultAsync(x => x.Id == billId, cancellationToken);
        if (bill is null)
        {
            return;
        }

        if (string.Equals(stripeEvent.Type, "checkout.session.completed", StringComparison.Ordinal))
        {
            bill.PaymentStatus = PaidStatus;
            bill.StripeSessionId = session.Id;
            bill.StripePaymentIntentId = session.PaymentIntentId ?? session.Id;
            bill.PaidAtUtc = DateTime.UtcNow;
            bill.UpdatedAt = DateTime.UtcNow;
        }
        else if (!string.Equals(bill.PaymentStatus, PaidStatus, StringComparison.OrdinalIgnoreCase))
        {
            bill.PaymentStatus = UnpaidStatus;
            bill.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static string? GetMetadataValue(Dictionary<string, string>? metadata, string key)
    {
        if (metadata is null)
        {
            return null;
        }

        return metadata.TryGetValue(key, out var value) ? value : null;
    }

    private static long ConvertMajorAmountToMinorUnits(decimal amount, string currency)
    {
        var rounded = ZeroDecimalCurrencies.Contains(currency)
            ? decimal.Round(amount, 0, MidpointRounding.AwayFromZero)
            : decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero);

        return Convert.ToInt64(rounded);
    }
}

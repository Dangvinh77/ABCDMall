using ABCDMall.Modules.Users.Application.Services.Bidding;
using ABCDMall.Modules.Users.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace ABCDMall.Modules.Users.Infrastructure.Services;

public sealed class BidStripePaymentGateway : IBidStripePaymentGateway
{
    private const string CheckoutSessionCompletedEvent = "checkout.session.completed";

    private static readonly HashSet<string> ZeroDecimalCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "bif", "clp", "djf", "gnf", "jpy", "kmf", "krw", "mga", "pyg", "rwf", "ugx", "vnd", "vuv", "xaf", "xof", "xpf"
    };

    private readonly StripeSettings _settings;

    public BidStripePaymentGateway(IOptions<StripeSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<BidStripeCheckoutSessionResult> CreateCheckoutSessionAsync(
        BidStripeCheckoutSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.SecretKey))
        {
            throw new InvalidOperationException("StripeSettings:SecretKey is missing.");
        }

        if (string.IsNullOrWhiteSpace(_settings.FrontendBaseUrl))
        {
            throw new InvalidOperationException("StripeSettings:FrontendBaseUrl is missing.");
        }

        StripeConfiguration.ApiKey = _settings.SecretKey;

        var frontendBaseUrl = _settings.FrontendBaseUrl.TrimEnd('/');
        var successUrl = $"{frontendBaseUrl}/manager-bidding/payment/success?bidId={Uri.EscapeDataString(request.BidId)}&session_id={{CHECKOUT_SESSION_ID}}";
        var cancelUrl = $"{frontendBaseUrl}/manager-bidding/payment/cancel?bidId={Uri.EscapeDataString(request.BidId)}";

        var options = new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            ExpiresAt = request.ExpiresAtUtc,
            CustomerEmail = request.CustomerEmail,
            Metadata = new Dictionary<string, string>
            {
                ["bidId"] = request.BidId,
                ["shopId"] = request.ShopId,
                ["templateType"] = request.TemplateType
            },
            PaymentMethodTypes = new List<string> { "card" },
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    ["bidId"] = request.BidId,
                    ["shopId"] = request.ShopId,
                    ["templateType"] = request.TemplateType
                },
                Description = $"ABCD Mall carousel bid for {request.ShopName}",
                ReceiptEmail = request.CustomerEmail
            },
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = request.Currency.ToLowerInvariant(),
                        UnitAmount = ConvertMajorAmountToMinorUnits(request.BidAmount, request.Currency),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"ABCD Mall carousel bid - {request.ShopName}",
                            Description = $"Homepage carousel placement bid ({request.TemplateType})"
                        }
                    }
                }
            }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options, cancellationToken: cancellationToken);

        if (string.IsNullOrWhiteSpace(session.Url))
        {
            throw new InvalidOperationException("Stripe Checkout session URL was not returned.");
        }

        return new BidStripeCheckoutSessionResult
        {
            SessionId = session.Id,
            CheckoutUrl = session.Url,
            ExpiresAtUtc = session.ExpiresAt
        };
    }

    public BidStripeWebhookEvent ParseWebhookEvent(
        string payload,
        string signatureHeader)
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

        if (string.Equals(stripeEvent.Type, CheckoutSessionCompletedEvent, StringComparison.Ordinal))
        {
            var session = stripeEvent.Data.Object as Session
                ?? throw new InvalidOperationException("Stripe webhook payload is not a Checkout Session.");

            return new BidStripeWebhookEvent
            {
                EventType = stripeEvent.Type,
                BidId = GetMetadataValue(session.Metadata, "bidId"),
                ShopId = GetMetadataValue(session.Metadata, "shopId"),
                ProviderTransactionId = session.PaymentIntentId ?? session.Id,
                Amount = session.AmountTotal.HasValue
                    ? ConvertMinorAmountToMajorUnits(session.AmountTotal.Value, session.Currency)
                    : null,
                Currency = session.Currency?.ToUpperInvariant(),
                RawPayload = payload
            };
        }

        return new BidStripeWebhookEvent
        {
            EventType = stripeEvent.Type,
            RawPayload = payload
        };
    }

    private static string? GetMetadataValue(
        Dictionary<string, string>? metadata,
        string key)
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

    private static decimal ConvertMinorAmountToMajorUnits(long amount, string? currency)
    {
        if (!string.IsNullOrWhiteSpace(currency) && ZeroDecimalCurrencies.Contains(currency))
        {
            return amount;
        }

        return decimal.Divide(amount, 100m);
    }
}

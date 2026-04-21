using ABCDMall.Modules.Movies.Application.Services.Payments;
using ABCDMall.Modules.Movies.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace ABCDMall.Modules.Movies.Infrastructure.Services.Payments;

public sealed class StripePaymentGateway : IStripePaymentGateway
{
    private const string CheckoutSessionCompletedEvent = "checkout.session.completed";
    private const string CheckoutSessionExpiredEvent = "checkout.session.expired";

    private static readonly HashSet<string> ZeroDecimalCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "bif", "clp", "djf", "gnf", "jpy", "kmf", "krw", "mga", "pyg", "rwf", "ugx", "vnd", "vuv", "xaf", "xof", "xpf"
    };

    private readonly StripeSettings _settings;

    public StripePaymentGateway(IOptions<StripeSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<StripeCheckoutSessionResult> CreateCheckoutSessionAsync(
        StripeCheckoutSessionRequest request,
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

        var booking = request.Booking;
        var frontendBaseUrl = _settings.FrontendBaseUrl.TrimEnd('/');
        var successUrl = $"{frontendBaseUrl}/movies/payment/success?bookingCode={Uri.EscapeDataString(booking.BookingCode)}&holdId={booking.BookingHoldId}&session_id={{CHECKOUT_SESSION_ID}}";
        var cancelUrl = $"{frontendBaseUrl}/movies/payment/cancel?bookingCode={Uri.EscapeDataString(booking.BookingCode)}&holdId={booking.BookingHoldId}";

        var options = new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            ExpiresAt = request.ExpiresAtUtc,
            CustomerEmail = booking.CustomerEmail,
            Metadata = new Dictionary<string, string>
            {
                ["bookingId"] = booking.Id.ToString(),
                ["bookingCode"] = booking.BookingCode,
                ["holdId"] = booking.BookingHoldId?.ToString() ?? string.Empty
            },
            PaymentMethodTypes = new List<string> { "card" },
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    ["bookingId"] = booking.Id.ToString(),
                    ["bookingCode"] = booking.BookingCode,
                    ["holdId"] = booking.BookingHoldId?.ToString() ?? string.Empty
                },
                Description = $"ABCD Cinema booking {booking.BookingCode}",
                ReceiptEmail = booking.CustomerEmail
            },
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = booking.Currency.ToLowerInvariant(),
                        UnitAmount = ConvertMajorAmountToMinorUnits(booking.GrandTotal, booking.Currency),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"ABCD Cinema booking {booking.BookingCode}",
                            Description = $"Movie ticket order {booking.BookingCode}"
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

        return new StripeCheckoutSessionResult
        {
            SessionId = session.Id,
            CheckoutUrl = session.Url,
            ExpiresAtUtc = session.ExpiresAt
        };
    }

    public StripeWebhookEvent ParseWebhookEvent(
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

            return new StripeWebhookEvent
            {
                EventType = stripeEvent.Type,
                BookingId = ParseGuid(GetMetadataValue(session.Metadata, "bookingId")),
                HoldId = ParseGuid(GetMetadataValue(session.Metadata, "holdId")),
                ProviderTransactionId = session.PaymentIntentId ?? session.Id,
                Amount = session.AmountTotal.HasValue
                    ? ConvertMinorAmountToMajorUnits(session.AmountTotal.Value, session.Currency)
                    : null,
                Currency = session.Currency?.ToUpperInvariant(),
                RawPayload = payload
            };
        }

        if (string.Equals(stripeEvent.Type, CheckoutSessionExpiredEvent, StringComparison.Ordinal))
        {
            var session = stripeEvent.Data.Object as Session
                ?? throw new InvalidOperationException("Stripe webhook payload is not a Checkout Session.");

            return new StripeWebhookEvent
            {
                EventType = stripeEvent.Type,
                BookingId = ParseGuid(GetMetadataValue(session.Metadata, "bookingId")),
                HoldId = ParseGuid(GetMetadataValue(session.Metadata, "holdId")),
                RawPayload = payload
            };
        }

        return new StripeWebhookEvent
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

    private static Guid? ParseGuid(string? value)
    {
        return Guid.TryParse(value, out var parsed) ? parsed : null;
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

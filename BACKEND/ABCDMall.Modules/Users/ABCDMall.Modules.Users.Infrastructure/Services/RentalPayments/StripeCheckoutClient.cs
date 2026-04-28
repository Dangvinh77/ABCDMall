using Stripe;
using Stripe.Checkout;

namespace ABCDMall.Modules.Users.Infrastructure.Services.RentalPayments;

public sealed class StripeCheckoutClient : IStripeCheckoutClient
{
    private static readonly HashSet<string> ZeroDecimalCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "bif", "clp", "djf", "gnf", "jpy", "kmf", "krw", "mga", "pyg", "rwf", "ugx", "vnd", "vuv", "xaf", "xof", "xpf"
    };

    public async Task<StripeCheckoutSessionResult> CreateSessionAsync(
        StripeCheckoutSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var options = new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = request.SuccessUrl,
            CancelUrl = request.CancelUrl,
            CustomerEmail = request.CustomerEmail,
            Metadata = request.Metadata,
            PaymentMethodTypes = new List<string> { "card" },
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                Metadata = request.Metadata,
                Description = request.Description,
                ReceiptEmail = request.CustomerEmail
            },
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = request.Currency,
                        UnitAmount = ConvertMajorAmountToMinorUnits(request.Amount, request.Currency),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = request.ProductName,
                            Description = request.Description
                        }
                    }
                }
            }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options, cancellationToken: cancellationToken);

        return new StripeCheckoutSessionResult
        {
            SessionId = session.Id,
            CheckoutUrl = session.Url ?? string.Empty
        };
    }

    private static long ConvertMajorAmountToMinorUnits(decimal amount, string currency)
    {
        var rounded = ZeroDecimalCurrencies.Contains(currency)
            ? decimal.Round(amount, 0, MidpointRounding.AwayFromZero)
            : decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero);

        return Convert.ToInt64(rounded);
    }
}

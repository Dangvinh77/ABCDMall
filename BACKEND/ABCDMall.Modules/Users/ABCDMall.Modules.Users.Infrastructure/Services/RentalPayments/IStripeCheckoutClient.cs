using Stripe.Checkout;

namespace ABCDMall.Modules.Users.Infrastructure.Services.RentalPayments;

public interface IStripeCheckoutClient
{
    Task<StripeCheckoutSessionResult> CreateSessionAsync(
        StripeCheckoutSessionRequest request,
        CancellationToken cancellationToken = default);
}

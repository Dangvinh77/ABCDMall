using ABCDMall.Modules.Movies.Domain.Entities;

namespace ABCDMall.Modules.Movies.Application.Services.Payments;

public interface IStripePaymentGateway
{
    Task<StripeCheckoutSessionResult> CreateCheckoutSessionAsync(
        StripeCheckoutSessionRequest request,
        CancellationToken cancellationToken = default);

    StripeWebhookEvent ParseWebhookEvent(
        string payload,
        string signatureHeader);
}

namespace ABCDMall.Modules.Users.Application.Services.Bidding;

public interface IBidStripePaymentGateway
{
    Task<BidStripeCheckoutSessionResult> CreateCheckoutSessionAsync(
        BidStripeCheckoutSessionRequest request,
        CancellationToken cancellationToken = default);

    BidStripeWebhookEvent ParseWebhookEvent(
        string payload,
        string signatureHeader);
}

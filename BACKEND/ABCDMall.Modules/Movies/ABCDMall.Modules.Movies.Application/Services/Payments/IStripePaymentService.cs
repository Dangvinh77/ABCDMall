using ABCDMall.Modules.Movies.Application.DTOs.Payments;

namespace ABCDMall.Modules.Movies.Application.Services.Payments;

public interface IStripePaymentService
{
    Task<StripeCheckoutSessionResponseDto> CreateCheckoutSessionAsync(
        CreateStripeCheckoutSessionRequestDto request,
        CancellationToken cancellationToken = default);

    Task ProcessWebhookAsync(
        string payload,
        string signatureHeader,
        CancellationToken cancellationToken = default);
}

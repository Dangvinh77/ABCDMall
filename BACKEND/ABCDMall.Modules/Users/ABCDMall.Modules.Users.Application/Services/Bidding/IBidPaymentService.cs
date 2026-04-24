using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Application.DTOs.Bidding;

namespace ABCDMall.Modules.Users.Application.Services.Bidding;

public interface IBidPaymentService
{
    Task<ApplicationResult<BidPaymentCheckoutSessionDto>> CreateCheckoutSessionAsync(
        string bidId,
        string shopId,
        CancellationToken cancellationToken = default);

    Task ProcessWebhookAsync(
        string payload,
        string signatureHeader,
        CancellationToken cancellationToken = default);
}

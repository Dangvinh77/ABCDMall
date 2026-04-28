using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Application.DTOs.RentalPayments;

namespace ABCDMall.Modules.Users.Application.Services.RentalPayments;

public interface IRentalPaymentService
{
    Task<ApplicationResult<RentalCheckoutSessionResponseDto>> CreateCheckoutSessionAsync(
        string billId,
        string managerUserId,
        string? managerShopId,
        CancellationToken cancellationToken = default);

    Task ProcessStripeWebhookAsync(
        string payload,
        string signatureHeader,
        CancellationToken cancellationToken = default);
}

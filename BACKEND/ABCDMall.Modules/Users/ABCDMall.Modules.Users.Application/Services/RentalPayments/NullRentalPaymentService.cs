using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Application.DTOs.RentalPayments;

namespace ABCDMall.Modules.Users.Application.Services.RentalPayments;

internal sealed class NullRentalPaymentService : IRentalPaymentService
{
    public Task<ApplicationResult<RentalCheckoutSessionResponseDto>> CreateCheckoutSessionAsync(
        string billId,
        string managerUserId,
        string? managerShopId,
        CancellationToken cancellationToken = default)
        => Task.FromResult(ApplicationResult<RentalCheckoutSessionResponseDto>.BadRequest("Rental payment service is not configured."));

    public Task ProcessStripeWebhookAsync(
        string payload,
        string signatureHeader,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

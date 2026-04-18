using ABCDMall.Modules.Movies.Domain.Enums;

namespace ABCDMall.Modules.Movies.Application.Services.Bookings;

public interface IPaymentRepository
{
    Task<PaymentProcessingResult> ApplyPaymentResultAsync(
        Guid bookingId,
        PaymentProvider provider,
        string providerTransactionId,
        PaymentStatus status,
        decimal amount,
        string currency,
        string? rawPayload,
        string? failureReason,
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    Task<PaymentProcessingResult?> GetStatusAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default);
}

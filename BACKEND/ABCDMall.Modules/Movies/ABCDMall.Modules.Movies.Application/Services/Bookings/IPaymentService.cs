using ABCDMall.Modules.Movies.Application.DTOs.Payments;

namespace ABCDMall.Modules.Movies.Application.Services.Bookings;

public interface IPaymentService
{
    Task<PaymentResponseDto> ProcessResultAsync(
        Guid bookingId,
        PaymentResultRequestDto request,
        CancellationToken cancellationToken = default);

    Task<PaymentStatusResponseDto?> GetStatusAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default);
}

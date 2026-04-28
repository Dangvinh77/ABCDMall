using ABCDMall.Modules.Movies.Application.DTOs.Payments;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;

namespace ABCDMall.Modules.Movies.Application.Services.Bookings;

public sealed class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;

    public PaymentService(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<PaymentResponseDto> ProcessResultAsync(
        Guid bookingId,
        PaymentResultRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<PaymentProvider>(request.Provider, ignoreCase: true, out var provider)
            || provider == PaymentProvider.Unknown)
        {
            throw new InvalidOperationException($"Unsupported payment provider: {request.Provider}.");
        }

        if (!Enum.TryParse<PaymentStatus>(request.Status, ignoreCase: true, out var status))
        {
            throw new InvalidOperationException($"Unsupported payment status: {request.Status}.");
        }

        var result = await _paymentRepository.ApplyPaymentResultAsync(
            bookingId,
            provider,
            request.ProviderTransactionId.Trim(),
            status,
            request.Amount,
            request.Currency.Trim().ToUpperInvariant(),
            request.RawPayload,
            request.FailureReason,
            DateTime.UtcNow,
            cancellationToken);

        return MapPaymentResponse(result.Payment, result.Booking);
    }

    public async Task<PaymentStatusResponseDto?> GetStatusAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        var result = await _paymentRepository.GetStatusAsync(paymentId, cancellationToken);
        if (result is null)
        {
            return null;
        }

        return new PaymentStatusResponseDto
        {
            PaymentId = result.Payment.Id,
            BookingId = result.Booking.Id,
            BookingCode = result.Booking.BookingCode,
            PaymentStatus = result.Payment.Status.ToString(),
            BookingStatus = result.Booking.Status.ToString(),
            Amount = result.Payment.Amount,
            Currency = result.Payment.Currency,
            CompletedAtUtc = result.Payment.CompletedAtUtc
        };
    }

    private static PaymentResponseDto MapPaymentResponse(Payment payment, Booking booking)
    {
        return new PaymentResponseDto
        {
            PaymentId = payment.Id,
            BookingId = booking.Id,
            BookingCode = booking.BookingCode,
            Provider = payment.Provider.ToString(),
            ProviderTransactionId = payment.ProviderTransactionId,
            Status = payment.Status.ToString(),
            Amount = payment.Amount,
            Currency = payment.Currency,
            BookingStatus = booking.Status.ToString(),
            FailureReason = payment.FailureReason,
            CreatedAtUtc = payment.CreatedAtUtc,
            UpdatedAtUtc = payment.UpdatedAtUtc,
            CompletedAtUtc = payment.CompletedAtUtc
        };
    }
}

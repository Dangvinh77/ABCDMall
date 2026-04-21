using ABCDMall.Modules.Movies.Application.DTOs.Payments;
using ABCDMall.Modules.Movies.Application.Services.Bookings;
using ABCDMall.Modules.Movies.Domain.Enums;

namespace ABCDMall.Modules.Movies.Application.Services.Payments;

public sealed class StripePaymentService : IStripePaymentService
{
    private static readonly TimeSpan CheckoutSessionDuration = TimeSpan.FromMinutes(35);

    private readonly IBookingRepository _bookingRepository;
    private readonly IBookingHoldRepository _bookingHoldRepository;
    private readonly IPaymentService _paymentService;
    private readonly IStripePaymentGateway _stripePaymentGateway;

    public StripePaymentService(
        IBookingRepository bookingRepository,
        IBookingHoldRepository bookingHoldRepository,
        IPaymentService paymentService,
        IStripePaymentGateway stripePaymentGateway)
    {
        _bookingRepository = bookingRepository;
        _bookingHoldRepository = bookingHoldRepository;
        _paymentService = paymentService;
        _stripePaymentGateway = stripePaymentGateway;
    }

    public async Task<StripeCheckoutSessionResponseDto> CreateCheckoutSessionAsync(
        CreateStripeCheckoutSessionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
        if (booking is null)
        {
            throw new InvalidOperationException("Booking not found.");
        }

        if (booking.Status != BookingStatus.PendingPayment)
        {
            throw new InvalidOperationException($"Booking is already {booking.Status}.");
        }

        if (!booking.BookingHoldId.HasValue)
        {
            throw new InvalidOperationException("Booking does not have a linked hold.");
        }

        var hold = await _bookingHoldRepository.GetByIdAsync(booking.BookingHoldId.Value, cancellationToken);
        if (hold is null)
        {
            throw new InvalidOperationException("Booking hold not found.");
        }

        if (hold.Status != BookingHoldStatus.Active)
        {
            throw new InvalidOperationException($"Booking hold is already {hold.Status}.");
        }

        var now = DateTime.UtcNow;
        if (hold.ExpiresAtUtc <= now)
        {
            await _bookingHoldRepository.ExpireAsync(now, cancellationToken);
            throw new InvalidOperationException("Booking hold has expired.");
        }

        var requiredExpiry = now.Add(CheckoutSessionDuration);
        var expiresAtUtc = hold.ExpiresAtUtc > requiredExpiry ? hold.ExpiresAtUtc : requiredExpiry;
        await _bookingHoldRepository.ExtendExpirationAsync(hold.Id, expiresAtUtc, cancellationToken);

        var session = await _stripePaymentGateway.CreateCheckoutSessionAsync(
            new StripeCheckoutSessionRequest
            {
                Booking = booking,
                ExpiresAtUtc = expiresAtUtc
            },
            cancellationToken);

        return new StripeCheckoutSessionResponseDto
        {
            BookingId = booking.Id,
            BookingCode = booking.BookingCode,
            HoldId = booking.BookingHoldId,
            SessionId = session.SessionId,
            CheckoutUrl = session.CheckoutUrl,
            ExpiresAtUtc = session.ExpiresAtUtc
        };
    }

    public async Task ProcessWebhookAsync(
        string payload,
        string signatureHeader,
        CancellationToken cancellationToken = default)
    {
        var webhookEvent = _stripePaymentGateway.ParseWebhookEvent(payload, signatureHeader);

        if (string.Equals(webhookEvent.EventType, "checkout.session.completed", StringComparison.OrdinalIgnoreCase))
        {
            if (!webhookEvent.BookingId.HasValue
                || string.IsNullOrWhiteSpace(webhookEvent.ProviderTransactionId)
                || !webhookEvent.Amount.HasValue
                || string.IsNullOrWhiteSpace(webhookEvent.Currency))
            {
                return;
            }

            await _paymentService.ProcessResultAsync(
                webhookEvent.BookingId.Value,
                new PaymentResultRequestDto
                {
                    Provider = PaymentProvider.Stripe.ToString(),
                    ProviderTransactionId = webhookEvent.ProviderTransactionId,
                    Status = PaymentStatus.Succeeded.ToString(),
                    Amount = webhookEvent.Amount.Value,
                    Currency = webhookEvent.Currency,
                    RawPayload = webhookEvent.RawPayload
                },
                cancellationToken);

            return;
        }

        if (string.Equals(webhookEvent.EventType, "checkout.session.expired", StringComparison.OrdinalIgnoreCase)
            && webhookEvent.HoldId.HasValue)
        {
            await _bookingHoldRepository.ReleaseAsync(webhookEvent.HoldId.Value, DateTime.UtcNow, cancellationToken);
        }
    }
}

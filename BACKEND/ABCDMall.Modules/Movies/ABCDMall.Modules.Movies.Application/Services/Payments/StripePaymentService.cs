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

        var seatInventoryIds = booking.Items
            .Where(item => string.Equals(item.ItemType, "Seat", StringComparison.OrdinalIgnoreCase) && item.SeatInventoryId.HasValue)
            .Select(item => item.SeatInventoryId!.Value)
            .Distinct()
            .ToArray();

        if (seatInventoryIds.Length == 0)
        {
            throw new InvalidOperationException("Booking does not have any linked seat holds.");
        }

        var now = DateTime.UtcNow;
        var holds = await _bookingHoldRepository.GetActiveByShowtimeAndSeatInventoryIdsAsync(
            booking.ShowtimeId,
            seatInventoryIds,
            now,
            cancellationToken);

        if (holds.Count == 0)
        {
            throw new InvalidOperationException("Booking hold not found.");
        }

        var coveredSeatInventoryIds = holds
            .SelectMany(hold => hold.Seats)
            .Select(seat => seat.SeatInventoryId)
            .Distinct()
            .ToHashSet();

        if (seatInventoryIds.Any(seatInventoryId => !coveredSeatInventoryIds.Contains(seatInventoryId)))
        {
            throw new InvalidOperationException("One or more booking holds are no longer active.");
        }

        if (holds.Any(hold => hold.ExpiresAtUtc <= now))
        {
            await _bookingHoldRepository.ExpireAsync(now, cancellationToken);
            throw new InvalidOperationException("Booking hold has expired.");
        }

        var requiredExpiry = now.Add(CheckoutSessionDuration);
        var expiresAtUtc = holds.Max(hold => hold.ExpiresAtUtc) > requiredExpiry
            ? holds.Max(hold => hold.ExpiresAtUtc)
            : requiredExpiry;

        foreach (var hold in holds)
        {
            await _bookingHoldRepository.ExtendExpirationAsync(hold.Id, expiresAtUtc, cancellationToken);
        }

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
            && webhookEvent.BookingId.HasValue)
        {
            var booking = await _bookingRepository.GetByIdAsync(webhookEvent.BookingId.Value, cancellationToken);
            if (booking is null)
            {
                return;
            }

            var seatInventoryIds = booking.Items
                .Where(item => string.Equals(item.ItemType, "Seat", StringComparison.OrdinalIgnoreCase) && item.SeatInventoryId.HasValue)
                .Select(item => item.SeatInventoryId!.Value)
                .Distinct()
                .ToArray();

            var activeHolds = await _bookingHoldRepository.GetActiveByShowtimeAndSeatInventoryIdsAsync(
                booking.ShowtimeId,
                seatInventoryIds,
                DateTime.UtcNow,
                cancellationToken);

            foreach (var hold in activeHolds)
            {
                await _bookingHoldRepository.ReleaseAsync(hold.Id, DateTime.UtcNow, cancellationToken);
            }
            return;
        }

        if (string.Equals(webhookEvent.EventType, "checkout.session.expired", StringComparison.OrdinalIgnoreCase)
            && webhookEvent.HoldId.HasValue)
        {
            await _bookingHoldRepository.ReleaseAsync(webhookEvent.HoldId.Value, DateTime.UtcNow, cancellationToken);
        }
    }
}

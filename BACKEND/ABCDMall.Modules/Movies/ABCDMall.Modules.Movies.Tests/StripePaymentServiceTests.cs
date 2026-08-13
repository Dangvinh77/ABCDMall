using ABCDMall.Modules.Movies.Application.DTOs.Payments;
using ABCDMall.Modules.Movies.Application.Services.Bookings;
using ABCDMall.Modules.Movies.Application.Services.Payments;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;
using Xunit;

namespace ABCDMall.Modules.Movies.Tests;

public sealed class StripePaymentServiceTests
{
    [Fact]
    public async Task CreateCheckoutSessionAsync_should_extend_every_active_hold_linked_to_the_booking()
    {
        var bookingRepository = new FakeBookingRepository();
        var holdRepository = new FakeBookingHoldRepository();
        var paymentService = new FakePaymentService();
        var gateway = new FakeStripePaymentGateway();
        var service = new StripePaymentService(bookingRepository, holdRepository, paymentService, gateway);

        var booking = bookingRepository.SeedPendingBooking();
        holdRepository.SeedActiveHoldsForBooking(booking);

        await service.CreateCheckoutSessionAsync(new CreateStripeCheckoutSessionRequestDto
        {
            BookingId = booking.Id
        }, CancellationToken.None);

        Assert.Contains(StripePaymentServiceTestIds.HoldA, holdRepository.ExtendedHoldIds);
        Assert.Contains(StripePaymentServiceTestIds.HoldB, holdRepository.ExtendedHoldIds);
    }

    [Fact]
    public async Task ProcessWebhookAsync_should_release_every_active_hold_when_checkout_session_expires()
    {
        var bookingRepository = new FakeBookingRepository();
        var holdRepository = new FakeBookingHoldRepository();
        var paymentService = new FakePaymentService();
        var gateway = new FakeStripePaymentGateway
        {
            WebhookEvent = new StripeWebhookEvent
            {
                EventType = "checkout.session.expired",
                BookingId = StripePaymentServiceTestIds.BookingId,
                HoldId = StripePaymentServiceTestIds.HoldA,
                RawPayload = "{}"
            }
        };
        var service = new StripePaymentService(bookingRepository, holdRepository, paymentService, gateway);

        var booking = bookingRepository.SeedPendingBooking();
        holdRepository.SeedActiveHoldsForBooking(booking);

        await service.ProcessWebhookAsync("{}", "sig", CancellationToken.None);

        Assert.Contains(StripePaymentServiceTestIds.HoldA, holdRepository.ReleasedHoldIds);
        Assert.Contains(StripePaymentServiceTestIds.HoldB, holdRepository.ReleasedHoldIds);
    }

    private sealed class FakeBookingRepository : IBookingRepository
    {
        private Bookingg? _booking;

        public Bookingg SeedPendingBooking()
        {
            _booking = new Bookingg
            {
                Id = StripePaymentServiceTestIds.BookingId,
                BookingCode = "BK-TEST-01",
                ShowtimeId = StripePaymentServiceTestIds.ShowtimeId,
                BookingHoldId = StripePaymentServiceTestIds.HoldA,
                Status = BookingStatus.PendingPayment,
                CustomerName = "Alice",
                CustomerEmail = "alice@example.com",
                CustomerPhoneNumber = "0900000000",
                GrandTotal = 190000,
                Currency = "VND",
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                Items =
                [
                    new BookingItem
                    {
                        Id = Guid.NewGuid(),
                        BookingId = StripePaymentServiceTestIds.BookingId,
                        ItemType = "Seat",
                        ItemCode = "A1",
                        Description = "Seat A1",
                        SeatInventoryId = StripePaymentServiceTestIds.SeatA1,
                        Quantity = 1,
                        UnitPrice = 85000,
                        LineTotal = 85000
                    },
                    new BookingItem
                    {
                        Id = Guid.NewGuid(),
                        BookingId = StripePaymentServiceTestIds.BookingId,
                        ItemType = "Seat",
                        ItemCode = "A2",
                        Description = "Seat A2",
                        SeatInventoryId = StripePaymentServiceTestIds.SeatA2,
                        Quantity = 1,
                        UnitPrice = 85000,
                        LineTotal = 85000
                    }
                ]
            };

            return _booking;
        }

        public Task<Bookingg?> GetByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
            => Task.FromResult(_booking?.Id == bookingId ? _booking : null);

        public Task<BookingHold?> GetHoldForBookingAsync(Guid holdId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<BookingHold>> GetHoldsForBookingAsync(IReadOnlyCollection<Guid> holdIds, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Bookingg?> GetByHoldIdAsync(Guid holdId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Bookingg?> GetByCombinedHoldIdsAsync(IReadOnlyCollection<Guid> holdIds, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Bookingg?> GetByCodeAsync(string bookingCode, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<GuestCustomer?> FindGuestCustomerAsync(string email, string phoneNumber, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Bookingg> AddPendingBookingAsync(Bookingg booking, GuestCustomer? newGuestCustomer, DateTime utcNow, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Bookingg> AddPendingBookingAsync(Bookingg booking, GuestCustomer? newGuestCustomer, IReadOnlyCollection<Guid> holdIds, DateTime utcNow, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FakeBookingHoldRepository : IBookingHoldRepository
    {
        private readonly Dictionary<Guid, BookingHold> _holds = [];

        public List<Guid> ExtendedHoldIds { get; } = [];
        public List<Guid> ReleasedHoldIds { get; } = [];

        public void SeedActiveHoldsForBooking(Bookingg booking)
        {
            _holds[StripePaymentServiceTestIds.HoldA] = CreateHold(
                StripePaymentServiceTestIds.HoldA,
                booking.ShowtimeId,
                StripePaymentServiceTestIds.SeatA1,
                "A1");
            _holds[StripePaymentServiceTestIds.HoldB] = CreateHold(
                StripePaymentServiceTestIds.HoldB,
                booking.ShowtimeId,
                StripePaymentServiceTestIds.SeatA2,
                "A2");
        }

        public Task<BookingHold> AddAsync(BookingHold hold, CancellationToken cancellationToken = default)
        {
            _holds[hold.Id] = hold;
            return Task.FromResult(hold);
        }

        public Task<BookingHold?> GetByIdAsync(Guid holdId, CancellationToken cancellationToken = default)
            => Task.FromResult(_holds.TryGetValue(holdId, out var hold) ? hold : null);

        public Task ExtendExpirationAsync(Guid holdId, DateTime expiresAtUtc, CancellationToken cancellationToken = default)
        {
            ExtendedHoldIds.Add(holdId);
            if (_holds.TryGetValue(holdId, out var hold))
            {
                hold.ExpiresAtUtc = expiresAtUtc;
            }

            return Task.CompletedTask;
        }

        public Task<BookingHold?> ConfirmAsync(Guid holdId, DateTime utcNow, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<bool> ReleaseAsync(Guid holdId, DateTime utcNow, CancellationToken cancellationToken = default)
        {
            ReleasedHoldIds.Add(holdId);
            if (_holds.TryGetValue(holdId, out var hold))
            {
                hold.Status = BookingHoldStatus.Released;
                hold.UpdatedAtUtc = utcNow;
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Task<int> ExpireAsync(DateTime utcNow, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task<IReadOnlySet<Guid>> GetActiveSeatInventoryIdsAsync(Guid showtimeId, DateTime utcNow, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlySet<Guid>>(
                _holds.Values
                    .Where(x => x.ShowtimeId == showtimeId && x.Status == BookingHoldStatus.Active && x.ExpiresAtUtc > utcNow)
                    .SelectMany(x => x.Seats)
                    .Select(x => x.SeatInventoryId)
                    .ToHashSet());

        public Task<IReadOnlyList<BookingHold>> GetActiveByShowtimeAndSeatInventoryIdsAsync(
            Guid showtimeId,
            IReadOnlyCollection<Guid> seatInventoryIds,
            DateTime utcNow,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<BookingHold>>(
                _holds.Values
                    .Where(x => x.ShowtimeId == showtimeId
                        && x.Status == BookingHoldStatus.Active
                        && x.ExpiresAtUtc > utcNow
                        && x.Seats.Any(seat => seatInventoryIds.Contains(seat.SeatInventoryId)))
                    .ToList());

        private static BookingHold CreateHold(Guid holdId, Guid showtimeId, Guid seatInventoryId, string seatCode)
        {
            return new BookingHold
            {
                Id = holdId,
                HoldCode = $"HOLD-{seatCode}",
                ShowtimeId = showtimeId,
                Status = BookingHoldStatus.Active,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10),
                SeatSubtotal = 85000,
                ServiceFee = 10000,
                GrandTotal = 95000,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                Seats =
                [
                    new BookingHoldSeat
                    {
                        Id = Guid.NewGuid(),
                        SeatInventoryId = seatInventoryId,
                        SeatCode = seatCode,
                        SeatType = SeatType.Regular.ToString(),
                        UnitPrice = 85000
                    }
                ]
            };
        }
    }

    private sealed class FakePaymentService : IPaymentService
    {
        public Task<PaymentResponseDto> ProcessResultAsync(Guid bookingId, PaymentResultRequestDto request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<PaymentStatusResponseDto?> GetStatusAsync(Guid paymentId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FakeStripePaymentGateway : IStripePaymentGateway
    {
        public StripeWebhookEvent WebhookEvent { get; set; } = new()
        {
            EventType = "checkout.session.completed",
            RawPayload = "{}"
        };

        public Task<StripeCheckoutSessionResult> CreateCheckoutSessionAsync(StripeCheckoutSessionRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new StripeCheckoutSessionResult
            {
                SessionId = "session-1",
                CheckoutUrl = "https://checkout.test/session-1",
                ExpiresAtUtc = request.ExpiresAtUtc
            });

        public StripeWebhookEvent ParseWebhookEvent(string payload, string signatureHeader)
            => WebhookEvent;
    }

    private static class StripePaymentServiceTestIds
    {
        public static readonly Guid BookingId = Guid.Parse("11111111-2222-3333-4444-555555555555");
        public static readonly Guid ShowtimeId = Guid.Parse("22222222-3333-4444-5555-666666666666");
        public static readonly Guid HoldA = Guid.Parse("33333333-4444-5555-6666-777777777771");
        public static readonly Guid HoldB = Guid.Parse("33333333-4444-5555-6666-777777777772");
        public static readonly Guid SeatA1 = Guid.Parse("44444444-5555-6666-7777-888888888881");
        public static readonly Guid SeatA2 = Guid.Parse("44444444-5555-6666-7777-888888888882");
    }
}

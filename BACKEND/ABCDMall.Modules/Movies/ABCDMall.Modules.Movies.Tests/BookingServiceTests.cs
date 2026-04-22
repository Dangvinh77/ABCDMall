using ABCDMall.Modules.Movies.Application.DTOs.Bookings;
using ABCDMall.Modules.Movies.Application.Services.Bookings;
using ABCDMall.Modules.Movies.Application.Services.Showtimes;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;
using Xunit;

namespace ABCDMall.Modules.Movies.Tests;

public sealed class BookingServiceTests
{
    [Fact]
    public async Task CreateAsync_should_create_one_booking_from_multiple_active_holds()
    {
        var repository = new FakeBookingRepository();
        var service = BuildService(repository);
        var holdIds = repository.SeedActiveHoldsForSameShowtime();

        var result = await service.CreateAsync(new CreateBookingRequestDto
        {
            HoldIds = holdIds,
            CustomerName = "Alice",
            CustomerEmail = "alice@example.com",
            CustomerPhoneNumber = "0900000000"
        }, CancellationToken.None);

        Assert.Equal(BookingServiceTestIds.ShowtimeId, result.ShowtimeId);
        Assert.Equal(holdIds.Count, result.HoldIds.Count);
        Assert.True(result.PaymentRequired);
    }

    [Fact]
    public async Task CreateAsync_should_fail_when_any_hold_is_expired()
    {
        var repository = new FakeBookingRepository();
        var service = BuildService(repository);
        var holdIds = repository.SeedOneActiveAndOneExpiredHold();

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateBookingRequestDto
            {
                HoldIds = holdIds,
                CustomerName = "Alice",
                CustomerEmail = "alice@example.com",
                CustomerPhoneNumber = "0900000000"
            }, CancellationToken.None));

        Assert.Contains("expired", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static BookingService BuildService(FakeBookingRepository repository)
    {
        return new BookingService(
            repository,
            new FakeShowtimeRepository(),
            new AllowBookingPolicy());
    }

    private sealed class FakeBookingRepository : IBookingRepository
    {
        private readonly List<BookingHold> _holds = [];
        private readonly List<Bookingg> _bookings = [];

        public IReadOnlyCollection<Guid> SeedActiveHoldsForSameShowtime()
        {
            var first = CreateHold(BookingServiceTestIds.HoldA, BookingServiceTestIds.SeatA1, "A1", BookingHoldStatus.Active, DateTime.UtcNow.AddMinutes(10));
            var second = CreateHold(BookingServiceTestIds.HoldB, BookingServiceTestIds.SeatA2, "A2", BookingHoldStatus.Active, DateTime.UtcNow.AddMinutes(10));
            _holds.AddRange([first, second]);
            return [first.Id, second.Id];
        }

        public IReadOnlyCollection<Guid> SeedOneActiveAndOneExpiredHold()
        {
            var active = CreateHold(BookingServiceTestIds.HoldA, BookingServiceTestIds.SeatA1, "A1", BookingHoldStatus.Active, DateTime.UtcNow.AddMinutes(10));
            var expired = CreateHold(BookingServiceTestIds.HoldB, BookingServiceTestIds.SeatA2, "A2", BookingHoldStatus.Active, DateTime.UtcNow.AddSeconds(-1));
            _holds.AddRange([active, expired]);
            return [active.Id, expired.Id];
        }

        public Task<Bookingg?> GetByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
            => Task.FromResult(_bookings.FirstOrDefault(x => x.Id == bookingId));

        public Task<BookingHold?> GetHoldForBookingAsync(Guid holdId, CancellationToken cancellationToken = default)
            => Task.FromResult(_holds.FirstOrDefault(x => x.Id == holdId));

        public Task<IReadOnlyList<BookingHold>> GetHoldsForBookingAsync(IReadOnlyCollection<Guid> holdIds, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<BookingHold>>(_holds.Where(x => holdIds.Contains(x.Id)).ToList());

        public Task<Bookingg?> GetByHoldIdAsync(Guid holdId, CancellationToken cancellationToken = default)
            => Task.FromResult(_bookings.FirstOrDefault(x => x.BookingHoldId == holdId));

        public Task<Bookingg?> GetByCombinedHoldIdsAsync(IReadOnlyCollection<Guid> holdIds, CancellationToken cancellationToken = default)
        {
            var primaryHoldId = holdIds.OrderBy(x => x).FirstOrDefault();
            return Task.FromResult(_bookings.FirstOrDefault(x => x.BookingHoldId == primaryHoldId));
        }

        public Task<Bookingg?> GetByCodeAsync(string bookingCode, CancellationToken cancellationToken = default)
            => Task.FromResult(_bookings.FirstOrDefault(x => x.BookingCode == bookingCode));

        public Task<GuestCustomer?> FindGuestCustomerAsync(string email, string phoneNumber, CancellationToken cancellationToken = default)
            => Task.FromResult<GuestCustomer?>(null);

        public Task<Bookingg> AddPendingBookingAsync(Bookingg booking, GuestCustomer? newGuestCustomer, DateTime utcNow, CancellationToken cancellationToken = default)
            => throw new NotSupportedException("Single-hold path should not be used by multi-hold tests.");

        public Task<Bookingg> AddPendingBookingAsync(Bookingg booking, GuestCustomer? newGuestCustomer, IReadOnlyCollection<Guid> holdIds, DateTime utcNow, CancellationToken cancellationToken = default)
        {
            _bookings.Add(booking);
            foreach (var hold in _holds.Where(x => holdIds.Contains(x.Id)))
            {
                hold.Status = BookingHoldStatus.Converted;
                hold.UpdatedAtUtc = utcNow;
            }

            return Task.FromResult(booking);
        }

        private static BookingHold CreateHold(Guid holdId, Guid seatId, string seatCode, BookingHoldStatus status, DateTime expiresAtUtc)
        {
            return new BookingHold
            {
                Id = holdId,
                HoldCode = $"HOLD-{seatCode}",
                ShowtimeId = BookingServiceTestIds.ShowtimeId,
                Status = status,
                ExpiresAtUtc = expiresAtUtc,
                SeatSubtotal = 85000,
                ComboSubtotal = 0,
                ServiceFee = 10000,
                DiscountAmount = 0,
                GrandTotal = 95000,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                Seats =
                [
                    new BookingHoldSeat
                    {
                        Id = Guid.NewGuid(),
                        SeatInventoryId = seatId,
                        SeatCode = seatCode,
                        SeatType = SeatType.Regular.ToString(),
                        UnitPrice = 85000
                    }
                ]
            };
        }
    }

    private sealed class FakeShowtimeRepository : IShowtimeRepository
    {
        public Task<IReadOnlyList<Showtime>> GetShowtimesAsync(Guid? movieId, Guid? cinemaId, DateOnly? businessDate, string? hallType, string? language, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Showtime>>(Array.Empty<Showtime>());

        public Task<Showtime?> GetShowtimeByIdAsync(Guid showtimeId, CancellationToken cancellationToken = default)
        {
            Showtime? showtime = showtimeId == BookingServiceTestIds.ShowtimeId
                ? new Showtime
                {
                    Id = BookingServiceTestIds.ShowtimeId,
                    Status = ShowtimeStatus.Open,
                    StartAtUtc = DateTime.UtcNow.AddHours(2),
                    BusinessDate = DateOnly.FromDateTime(DateTime.UtcNow)
                }
                : null;

            return Task.FromResult(showtime);
        }

        public Task<IReadOnlyList<ShowtimeSeatInventory>> GetSeatMapByShowtimeIdAsync(Guid showtimeId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ShowtimeSeatInventory>>(Array.Empty<ShowtimeSeatInventory>());

        public Task MarkSeatsBookedAsync(Guid showtimeId, IReadOnlyCollection<Guid> seatInventoryIds, DateTime utcNow, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class AllowBookingPolicy : IShowtimeBookingPolicy
    {
        public ShowtimeBookingDecision EvaluateForUser(Showtime showtime, DateTime utcNow)
            => new ShowtimeBookingDecision { IsBookable = true };

        public bool IsVisibleForUser(Showtime showtime, DateTime utcNow)
            => true;

        public void EnsureBookableForUser(Showtime showtime, DateTime utcNow)
        {
        }
    }

    private static class BookingServiceTestIds
    {
        public static readonly Guid ShowtimeId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        public static readonly Guid HoldA = Guid.Parse("bbbbbbbb-1111-1111-1111-111111111111");
        public static readonly Guid HoldB = Guid.Parse("bbbbbbbb-2222-2222-2222-222222222222");
        public static readonly Guid SeatA1 = Guid.Parse("cccccccc-1111-1111-1111-111111111111");
        public static readonly Guid SeatA2 = Guid.Parse("cccccccc-2222-2222-2222-222222222222");
    }
}

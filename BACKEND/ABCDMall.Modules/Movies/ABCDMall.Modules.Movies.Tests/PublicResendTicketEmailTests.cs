using ABCDMall.Modules.Movies.Application.DTOs.Bookings;
using ABCDMall.Modules.Movies.Application.Services.Bookings;
using ABCDMall.Modules.Movies.Application.Services.Showtimes;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;
using Xunit;

namespace ABCDMall.Modules.Movies.Tests;

public sealed class PublicResendTicketEmailTests
{
    [Fact]
    public async Task ResendTicketEmailAsync_should_fail_when_booking_does_not_exist()
    {
        var repository = new FakeBookingRepository();
        var service = BuildService(repository);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ResendTicketEmailAsync(new ResendTicketEmailRequestDto
            {
                Email = "guest@example.com",
                BookingCode = "BK-404"
            }, CancellationToken.None));

        Assert.Equal("Booking does not exist.", error.Message);
    }

    [Fact]
    public async Task ResendTicketEmailAsync_should_fail_when_email_does_not_match()
    {
        var repository = new FakeBookingRepository();
        repository.SeedBooking("BK-123", "real@example.com", BookingStatus.Confirmed);
        var service = BuildService(repository);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ResendTicketEmailAsync(new ResendTicketEmailRequestDto
            {
                Email = "wrong@example.com",
                BookingCode = "BK-123"
            }, CancellationToken.None));

        Assert.Equal("Email does not match this booking.", error.Message);
    }

    [Fact]
    public async Task ResendTicketEmailAsync_should_fail_when_booking_is_not_eligible()
    {
        var repository = new FakeBookingRepository();
        repository.SeedBooking("BK-124", "guest@example.com", BookingStatus.PendingPayment);
        var service = BuildService(repository);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ResendTicketEmailAsync(new ResendTicketEmailRequestDto
            {
                Email = "guest@example.com",
                BookingCode = "BK-124"
            }, CancellationToken.None));

        Assert.Equal("This booking is not eligible for ticket resend.", error.Message);
    }

    [Fact]
    public async Task ResendTicketEmailAsync_should_trigger_ticket_resend_when_request_is_valid()
    {
        var repository = new FakeBookingRepository();
        var bookingId = repository.SeedBooking("BK-125", "guest@example.com", BookingStatus.Confirmed);
        var service = BuildService(repository);

        var result = await service.ResendTicketEmailAsync(new ResendTicketEmailRequestDto
        {
            Email = "guest@example.com",
            BookingCode = "BK-125"
        }, CancellationToken.None);

        Assert.Equal("Ticket email resent successfully.", result.Message);
        Assert.Equal(bookingId, repository.LastResentBookingId);
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
        private readonly List<Bookingg> _bookings = [];

        public Guid? LastResentBookingId { get; private set; }

        public Guid SeedBooking(string bookingCode, string email, BookingStatus status)
        {
            var booking = new Bookingg
            {
                Id = Guid.NewGuid(),
                BookingCode = bookingCode,
                CustomerEmail = email,
                CustomerName = "Guest",
                CustomerPhoneNumber = "0900000000",
                Status = status,
                ShowtimeId = Guid.NewGuid(),
                GuestCustomerId = Guid.NewGuid(),
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                Currency = "VND"
            };

            _bookings.Add(booking);
            return booking.Id;
        }

        public Task<Bookingg?> GetByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
            => Task.FromResult(_bookings.FirstOrDefault(x => x.Id == bookingId));

        public Task<BookingHold?> GetHoldForBookingAsync(Guid holdId, CancellationToken cancellationToken = default)
            => Task.FromResult<BookingHold?>(null);

        public Task<IReadOnlyList<BookingHold>> GetHoldsForBookingAsync(IReadOnlyCollection<Guid> holdIds, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<BookingHold>>(Array.Empty<BookingHold>());

        public Task<Bookingg?> GetByHoldIdAsync(Guid holdId, CancellationToken cancellationToken = default)
            => Task.FromResult<Bookingg?>(null);

        public Task<Bookingg?> GetByCombinedHoldIdsAsync(IReadOnlyCollection<Guid> holdIds, CancellationToken cancellationToken = default)
            => Task.FromResult<Bookingg?>(null);

        public Task<Bookingg?> GetByCodeAsync(string bookingCode, CancellationToken cancellationToken = default)
            => Task.FromResult(_bookings.FirstOrDefault(x => x.BookingCode == bookingCode));

        public Task<GuestCustomer?> FindGuestCustomerAsync(string email, string phoneNumber, CancellationToken cancellationToken = default)
            => Task.FromResult<GuestCustomer?>(null);

        public Task<Bookingg> AddPendingBookingAsync(Bookingg booking, GuestCustomer? newGuestCustomer, DateTime utcNow, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Bookingg> AddPendingBookingAsync(Bookingg booking, GuestCustomer? newGuestCustomer, IReadOnlyCollection<Guid> holdIds, DateTime utcNow, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task ResendTicketEmailAsync(Guid bookingId, CancellationToken cancellationToken = default)
        {
            LastResentBookingId = bookingId;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeShowtimeRepository : IShowtimeRepository
    {
        public Task<IReadOnlyList<Showtime>> GetShowtimesAsync(Guid? movieId, Guid? cinemaId, DateOnly? businessDate, string? hallType, string? language, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Showtime>>(Array.Empty<Showtime>());

        public Task<Showtime?> GetShowtimeByIdAsync(Guid showtimeId, CancellationToken cancellationToken = default)
            => Task.FromResult<Showtime?>(null);

        public Task<IReadOnlyList<ShowtimeSeatInventory>> GetSeatMapByShowtimeIdAsync(Guid showtimeId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ShowtimeSeatInventory>>(Array.Empty<ShowtimeSeatInventory>());

        public Task MarkSeatsBookedAsync(Guid showtimeId, IReadOnlyCollection<Guid> seatInventoryIds, DateTime utcNow, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class AllowBookingPolicy : IShowtimeBookingPolicy
    {
        public ShowtimeBookingDecision EvaluateForUser(Showtime showtime, DateTime utcNow)
            => new() { IsBookable = true };

        public bool IsVisibleForUser(Showtime showtime, DateTime utcNow)
            => true;

        public void EnsureBookableForUser(Showtime showtime, DateTime utcNow)
        {
        }
    }
}

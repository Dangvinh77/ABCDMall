using ABCDMall.Modules.Movies.Application.Services.Bookings;
using ABCDMall.Modules.Movies.Domain.Entities;

namespace ABCDMall.Modules.Movies.Tests;

internal sealed class FakeBookingHoldRepository : IBookingHoldRepository
{
    private readonly List<BookingHold> _holds = [];

    public Task<BookingHold> AddAsync(BookingHold hold, CancellationToken cancellationToken = default)
    {
        _holds.Add(hold);
        return Task.FromResult(hold);
    }

    public Task<BookingHold?> GetByIdAsync(Guid holdId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_holds.FirstOrDefault(x => x.Id == holdId));
    }

    public Task<bool> ReleaseAsync(Guid holdId, DateTime utcNow, CancellationToken cancellationToken = default)
    {
        var hold = _holds.FirstOrDefault(x => x.Id == holdId);
        if (hold is null || hold.Status != Domain.Enums.BookingHoldStatus.Active)
        {
            return Task.FromResult(false);
        }

        hold.Status = Domain.Enums.BookingHoldStatus.Released;
        hold.UpdatedAtUtc = utcNow;
        return Task.FromResult(true);
    }

    public Task ExtendExpirationAsync(Guid holdId, DateTime expiresAtUtc, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<int> ExpireAsync(DateTime utcNow, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<IReadOnlySet<Guid>> GetActiveSeatInventoryIdsAsync(
        Guid showtimeId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        IReadOnlySet<Guid> emptySet = new HashSet<Guid>();
        return Task.FromResult(emptySet);
    }

    public Task<IReadOnlyList<BookingHold>> GetActiveByShowtimeAndSeatInventoryIdsAsync(
        Guid showtimeId,
        IReadOnlyCollection<Guid> seatInventoryIds,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<BookingHold> holds = _holds
            .Where(hold => hold.ShowtimeId == showtimeId
                && hold.Status == Domain.Enums.BookingHoldStatus.Active
                && hold.ExpiresAtUtc > utcNow
                && hold.Seats.Any(seat => seatInventoryIds.Contains(seat.SeatInventoryId)))
            .ToList();

        return Task.FromResult(holds);
    }

    public Task<BookingHold?> ConfirmAsync(Guid holdId, DateTime utcNow, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

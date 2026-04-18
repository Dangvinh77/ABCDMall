using ABCDMall.Modules.Movies.Application.Services.Bookings;
using ABCDMall.Modules.Movies.Domain.Entities;

namespace ABCDMall.Modules.Movies.Tests;

internal sealed class FakeBookingHoldRepository : IBookingHoldRepository
{
    public Task<BookingHold> AddAsync(BookingHold hold, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<BookingHold?> GetByIdAsync(Guid holdId, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<bool> ReleaseAsync(Guid holdId, DateTime utcNow, CancellationToken cancellationToken = default)
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

    public Task<BookingHold?> ConfirmAsync(Guid holdId, DateTime utcNow, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

using ABCDMall.Modules.Movies.Domain.Entities;

namespace ABCDMall.Modules.Movies.Application.Services.Bookings;

public interface IBookingHoldRepository
{
    Task<BookingHold> AddAsync(BookingHold hold, CancellationToken cancellationToken = default);
    Task<BookingHold?> GetByIdAsync(Guid holdId, CancellationToken cancellationToken = default);
    Task<bool> ReleaseAsync(Guid holdId, DateTime utcNow, CancellationToken cancellationToken = default);
    Task<int> ExpireAsync(DateTime utcNow, CancellationToken cancellationToken = default);
}

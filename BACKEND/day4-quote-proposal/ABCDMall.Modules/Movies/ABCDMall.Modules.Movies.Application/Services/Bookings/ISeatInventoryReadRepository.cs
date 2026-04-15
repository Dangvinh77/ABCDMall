using ABCDMall.Modules.Movies.Application.Services.Bookings.Models;

namespace ABCDMall.Modules.Movies.Application.Services.Bookings;

public interface ISeatInventoryReadRepository
{
    Task<IReadOnlyList<SeatInventoryQuoteSnapshot>> GetSeatsByIdsAsync(
        Guid showtimeId,
        IReadOnlyCollection<Guid> seatInventoryIds,
        CancellationToken cancellationToken = default);
}

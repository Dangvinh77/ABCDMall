using ABCDMall.Modules.Movies.Application.Services.Bookings;

namespace ABCDMall.Modules.Movies.Application.Services.Bookings;

public interface ISeatInventoryReadRepository
{
    // Interface giúp services đọc thông tin kho ghế (seat inventory) theo showtime id và danh sách seat inventory id.
    Task<IReadOnlyList<SeatInventoryQuoteSnapshot>> GetSeatsByIdsAsync(
        Guid showtimeId,
        IReadOnlyCollection<Guid> seatInventoryIds,
        CancellationToken cancellationToken = default);
}

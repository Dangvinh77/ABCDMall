using ABCDMall.Modules.Movies.Application.Services.Bookings;

namespace ABCDMall.Modules.Movies.Application.Services.Bookings;

public interface IShowtimeReadRepository
{
    // Interface giúp services đọc showtime theo id.
    Task<ShowtimeQuoteSnapshot?> GetShowtimeByIdAsync(Guid showtimeId, CancellationToken cancellationToken = default);
}

using ABCDMall.Modules.Movies.Application.Services.Bookings.Models;

namespace ABCDMall.Modules.Movies.Application.Services.Bookings;

public interface IShowtimeReadRepository
{
    Task<ShowtimeQuoteSnapshot?> GetShowtimeByIdAsync(Guid showtimeId, CancellationToken cancellationToken = default);
}

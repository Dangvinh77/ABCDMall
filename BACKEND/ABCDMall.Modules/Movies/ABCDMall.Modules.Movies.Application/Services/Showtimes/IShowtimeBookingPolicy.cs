using ABCDMall.Modules.Movies.Domain.Entities;

namespace ABCDMall.Modules.Movies.Application.Services.Showtimes;

public interface IShowtimeBookingPolicy
{
    ShowtimeBookingDecision EvaluateForUser(Showtime showtime, DateTime utcNow);

    bool IsVisibleForUser(Showtime showtime, DateTime utcNow);

    void EnsureBookableForUser(Showtime showtime, DateTime utcNow);
}

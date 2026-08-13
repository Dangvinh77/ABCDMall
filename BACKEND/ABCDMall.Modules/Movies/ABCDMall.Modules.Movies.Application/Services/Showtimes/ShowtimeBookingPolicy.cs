using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;

namespace ABCDMall.Modules.Movies.Application.Services.Showtimes;

public sealed class ShowtimeBookingPolicy : IShowtimeBookingPolicy
{
    private static readonly TimeSpan BookingCutoff = TimeSpan.FromMinutes(10);
    private const int PublicScheduleDays = 7;

    public ShowtimeBookingDecision EvaluateForUser(Showtime showtime, DateTime utcNow)
    {
        if (!IsVisibleForUser(showtime, utcNow))
        {
            return NotBookable("This showtime is outside the public booking window.");
        }

        if (showtime.Status != ShowtimeStatus.Open)
        {
            return NotBookable("This showtime is not open for booking.");
        }

        if (showtime.EndAtUtc.HasValue && showtime.EndAtUtc.Value <= utcNow)
        {
            return NotBookable("This showtime has ended.");
        }

        if (showtime.StartAtUtc <= utcNow)
        {
            return NotBookable("This showtime has already started.");
        }

        if (showtime.StartAtUtc <= utcNow.Add(BookingCutoff))
        {
            return NotBookable("Online booking is closed for this showtime.");
        }

        return new ShowtimeBookingDecision { IsBookable = true };
    }

    public bool IsVisibleForUser(Showtime showtime, DateTime utcNow)
    {
        var today = GetVietnamDate(utcNow);
        var lastPublicDate = today.AddDays(PublicScheduleDays - 1);

        return showtime.BusinessDate >= today && showtime.BusinessDate <= lastPublicDate;
    }

    public void EnsureBookableForUser(Showtime showtime, DateTime utcNow)
    {
        var decision = EvaluateForUser(showtime, utcNow);
        if (!decision.IsBookable)
        {
            throw new InvalidOperationException(decision.UnavailableReason ?? "This showtime is not available for booking.");
        }
    }

    private static DateOnly GetVietnamDate(DateTime utcNow)
    {
        // Vietnam has no daylight-saving time, so UTC+7 is stable for the public booking window.
        return DateOnly.FromDateTime(utcNow.AddHours(7));
    }

    private static ShowtimeBookingDecision NotBookable(string reason)
    {
        return new ShowtimeBookingDecision
        {
            IsBookable = false,
            UnavailableReason = reason
        };
    }
}

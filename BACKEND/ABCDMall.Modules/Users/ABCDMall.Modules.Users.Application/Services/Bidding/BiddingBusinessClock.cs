namespace ABCDMall.Modules.Users.Application.Services.Bidding;

public static class BiddingBusinessClock
{
    private const string WindowsVietnamTimeZone = "SE Asia Standard Time";
    private const string IanaVietnamTimeZone = "Asia/Ho_Chi_Minh";

    public static TimeZoneInfo VietnamTimeZone { get; } = ResolveVietnamTimeZone();

    public static DateTime GetLocalNow(DateTime utcNow)
        => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc), VietnamTimeZone);

    public static DateTime GetCurrentWeekMonday(DateTime utcNow)
    {
        var localNow = GetLocalNow(utcNow);
        var date = localNow.Date;
        var offset = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-offset);
    }

    public static DateTime GetUpcomingWeekMonday(DateTime utcNow)
        => GetCurrentWeekMonday(utcNow).AddDays(7);

    public static DateTime ConvertLocalToUtc(DateTime localDateTime)
    {
        var unspecified = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(unspecified, VietnamTimeZone);
    }

    private static TimeZoneInfo ResolveVietnamTimeZone()
    {
        foreach (var id in new[] { WindowsVietnamTimeZone, IanaVietnamTimeZone })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.Utc;
    }
}

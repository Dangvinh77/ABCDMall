using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;

namespace ABCDMall.Modules.Movies.Application.Services.Promotions;

internal static class PromotionShowtimeRuleMatcher
{
    private static readonly TimeSpan VietnamUtcOffset = TimeSpan.FromHours(7);

    public static bool MatchesShowtimeContext(
        PromotionRule rule,
        Guid showtimeId,
        DateOnly businessDate,
        DateTime showtimeStartAtUtc)
    {
        return rule.RuleType switch
        {
            PromotionRuleType.Showtime => MatchesShowtimeRule(rule.RuleValue, showtimeId, showtimeStartAtUtc),
            PromotionRuleType.BusinessDate => MatchesBusinessDateRule(rule.RuleValue, businessDate),
            _ => true
        };
    }

    private static bool MatchesShowtimeRule(string ruleValue, Guid showtimeId, DateTime showtimeStartAtUtc)
    {
        if (Guid.TryParse(ruleValue, out var ruleShowtimeId))
        {
            return ruleShowtimeId == showtimeId;
        }

        var localTime = showtimeStartAtUtc + VietnamUtcOffset;
        var normalizedValue = ruleValue.Trim();

        if (string.Equals(normalizedValue, "Morning", StringComparison.OrdinalIgnoreCase))
        {
            var timeOfDay = TimeOnly.FromDateTime(localTime);
            return timeOfDay >= new TimeOnly(9, 0) && timeOfDay < new TimeOnly(11, 0);
        }

        if (string.Equals(normalizedValue, "Afternoon", StringComparison.OrdinalIgnoreCase))
        {
            var timeOfDay = TimeOnly.FromDateTime(localTime);
            return timeOfDay >= new TimeOnly(11, 0) && timeOfDay < new TimeOnly(17, 0);
        }

        if (string.Equals(normalizedValue, "Evening", StringComparison.OrdinalIgnoreCase))
        {
            var timeOfDay = TimeOnly.FromDateTime(localTime);
            return timeOfDay >= new TimeOnly(17, 0);
        }

        if (TryParseTimeWindow(normalizedValue, out var start, out var end))
        {
            var timeOfDay = TimeOnly.FromDateTime(localTime);
            return timeOfDay >= start && timeOfDay < end;
        }

        return false;
    }

    private static bool MatchesBusinessDateRule(string ruleValue, DateOnly businessDate)
    {
        if (string.Equals(ruleValue, "Weekend", StringComparison.OrdinalIgnoreCase))
        {
            return businessDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
        }

        if (DateOnly.TryParse(ruleValue, out var exactDate))
        {
            return exactDate == businessDate;
        }

        return true;
    }

    private static bool TryParseTimeWindow(string value, out TimeOnly start, out TimeOnly end)
    {
        start = default;
        end = default;

        var segments = value.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return segments.Length == 2
            && TimeOnly.TryParse(segments[0], out start)
            && TimeOnly.TryParse(segments[1], out end);
    }
}

using System.Globalization;

namespace OrderPoint.Admin.Shared.Extensions;

internal static class DateTimeOffsetExtensions
{
    internal static string ToRelativeTime(this DateTimeOffset dateTime)
    {
        TimeSpan elapsed = DateTimeOffset.UtcNow - dateTime;

        return elapsed switch
        {
            { TotalMinutes: < 1 } => "just now",
            { TotalHours: < 1 } => $"{(int)elapsed.TotalMinutes} min ago",
            { TotalDays: < 1 } => $"{(int)elapsed.TotalHours} h ago",
            { TotalDays: < 2 } => "1 day ago",
            _ => $"{(int)elapsed.TotalDays} days ago"
        };
    }

    internal static DateTimeOffset ToTimeZone(this DateTimeOffset dateTime, TimeZoneInfo timeZone)
    {
        return TimeZoneInfo.ConvertTime(dateTime, timeZone);
    }

    internal static string ToDisplayDateTime(this DateTimeOffset dateTime, TimeZoneInfo timeZone)
    {
        DateTimeOffset localDateTime = dateTime.ToTimeZone(timeZone);
        DateTime today = DateTimeOffset.UtcNow.ToTimeZone(timeZone).Date;
        string time = localDateTime.ToString("t", CultureInfo.CurrentCulture);

        if (localDateTime.Date == today)
        {
            return $"Today {time}";
        }

        if (localDateTime.Date == today.AddDays(-1))
        {
            return $"Yesterday {time}";
        }

        return localDateTime.ToString("g", CultureInfo.CurrentCulture);
    }
}
namespace OrderPoint.Bartender.Shared.Extensions;

internal static class TimeZoneInfoExtensions
{
    // Local midnight of the viewer's today, in UTC
    internal static DateTimeOffset GetTodayStartUtc(this TimeZoneInfo timeZone)
    {
        DateTime localToday = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZone).Date;

        return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(localToday, DateTimeKind.Unspecified), timeZone);
    }
}
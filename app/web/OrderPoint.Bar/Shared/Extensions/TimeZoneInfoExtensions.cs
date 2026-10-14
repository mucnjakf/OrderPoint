namespace OrderPoint.Bar.Shared.Extensions;

internal static class TimeZoneInfoExtensions
{
    // The start of the viewer's business day, in UTC: before the day start it is still yesterday's
    internal static DateTimeOffset GetTodayStartUtc(this TimeZoneInfo timeZone, TimeSpan dayStart)
    {
        DateTime localNow = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZone);
        DateTime localDayStart = (localNow - dayStart).Date + dayStart;

        return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(localDayStart, DateTimeKind.Unspecified), timeZone);
    }
}
using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Queries.Dashboard;

internal sealed record DashboardPeriodRange(
    DateTimeOffset FromUtc,
    DateTimeOffset ToUtc,
    DateTimeOffset PreviousFromUtc,
    DateTimeOffset PreviousToUtc,
    DateTime FromLocal,
    int DaysCount)
{
    // Days start at the business day's start time in the viewer's time zone, e.g. 06:00, so a night belongs to one day
    internal static DashboardPeriodRange For(
        DashboardPeriod period,
        DateTimeOffset nowUtc,
        TimeZoneInfo timeZone,
        TimeSpan dayStart)
    {
        int daysCount = period switch
        {
            DashboardPeriod.Today => 1,
            DashboardPeriod.Last7Days => 7,
            DashboardPeriod.Last30Days => 30,
            _ => throw new ArgumentOutOfRangeException(nameof(period), period, null)
        };

        DateTime fromLocal = GetLocalToday(nowUtc, timeZone, dayStart).AddDays(1 - daysCount) + dayStart;
        DateTimeOffset fromUtc = LocalToUtc(fromLocal, timeZone);
        DateTimeOffset previousFromUtc = LocalToUtc(fromLocal.AddDays(-daysCount), timeZone);
        TimeSpan length = nowUtc - fromUtc;

        return new DashboardPeriodRange(
            fromUtc,
            nowUtc,
            previousFromUtc,
            previousFromUtc + length,
            fromLocal,
            daysCount);
    }

    // The date of the business day now: before the day start it is still yesterday
    internal static DateTime GetLocalToday(DateTimeOffset nowUtc, TimeZoneInfo timeZone, TimeSpan dayStart)
        => (TimeZoneInfo.ConvertTime(nowUtc, timeZone).DateTime - dayStart).Date;

    internal static DateTime UtcToLocal(DateTimeOffset dateTimeUtc, TimeZoneInfo timeZone)
        => TimeZoneInfo.ConvertTime(dateTimeUtc, timeZone).DateTime;

    internal static DateTimeOffset LocalToUtc(DateTime localDateTime, TimeZoneInfo timeZone)
        => new DateTimeOffset(localDateTime, timeZone.GetUtcOffset(localDateTime)).ToUniversalTime();
}
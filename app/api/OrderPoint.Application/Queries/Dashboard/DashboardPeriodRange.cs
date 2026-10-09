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
    internal static DashboardPeriodRange For(DashboardPeriod period, DateTimeOffset nowUtc, TimeZoneInfo timeZone)
    {
        int daysCount = period switch
        {
            DashboardPeriod.Today => 1,
            DashboardPeriod.Last7Days => 7,
            DashboardPeriod.Last30Days => 30,
            _ => throw new ArgumentOutOfRangeException(nameof(period), period, null)
        };

        DateTime fromLocal = GetLocalToday(nowUtc, timeZone).AddDays(1 - daysCount);
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

    internal static DateTime GetLocalToday(DateTimeOffset nowUtc, TimeZoneInfo timeZone)
        => TimeZoneInfo.ConvertTime(nowUtc, timeZone).Date;

    internal static DateTime UtcToLocal(DateTimeOffset dateTimeUtc, TimeZoneInfo timeZone)
        => TimeZoneInfo.ConvertTime(dateTimeUtc, timeZone).DateTime;

    internal static DateTimeOffset LocalToUtc(DateTime localDateTime, TimeZoneInfo timeZone)
        => new DateTimeOffset(localDateTime, timeZone.GetUtcOffset(localDateTime)).ToUniversalTime();
}
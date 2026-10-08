using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Queries.Dashboard;

internal sealed record DashboardPeriodRange(
    DateTimeOffset FromUtc,
    DateTimeOffset ToUtc,
    DateTimeOffset PreviousFromUtc,
    DateTimeOffset PreviousToUtc,
    int DaysCount)
{
    internal static DashboardPeriodRange For(DashboardPeriod period, DateTimeOffset nowUtc)
    {
        int daysCount = period switch
        {
            DashboardPeriod.Today => 1,
            DashboardPeriod.Last7Days => 7,
            DashboardPeriod.Last30Days => 30,
            _ => throw new ArgumentOutOfRangeException(nameof(period), period, null)
        };

        var todayStartUtc = new DateTimeOffset(nowUtc.UtcDateTime.Date, TimeSpan.Zero);
        DateTimeOffset fromUtc = todayStartUtc.AddDays(1 - daysCount);
        TimeSpan length = nowUtc - fromUtc;
        DateTimeOffset previousFromUtc = fromUtc.AddDays(-daysCount);

        return new DashboardPeriodRange(fromUtc, nowUtc, previousFromUtc, previousFromUtc + length, daysCount);
    }
}
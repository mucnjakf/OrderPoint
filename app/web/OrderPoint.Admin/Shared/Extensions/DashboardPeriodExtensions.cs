using OrderPoint.Admin.Dashboard.Enumerations;

namespace OrderPoint.Admin.Shared.Extensions;

internal static class DashboardPeriodExtensions
{
    internal static string GetLabel(this DashboardPeriod period)
    {
        return period switch
        {
            DashboardPeriod.Today => "Today",
            DashboardPeriod.Last7Days => "7 days",
            DashboardPeriod.Last30Days => "30 days",
            _ => throw new ArgumentOutOfRangeException(nameof(period), period, null)
        };
    }
}
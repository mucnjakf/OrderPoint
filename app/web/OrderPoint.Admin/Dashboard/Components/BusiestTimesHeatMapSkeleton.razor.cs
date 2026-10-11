namespace OrderPoint.Admin.Dashboard.Components;

public sealed partial class BusiestTimesHeatMapSkeleton
{
    private const int DaysCount = 7;

    private const int HoursPerDay = 24;

    private const int LabelHourStep = 3;

    private static string DayLabelStyle => "width: 36px; flex-shrink: 0;";
}
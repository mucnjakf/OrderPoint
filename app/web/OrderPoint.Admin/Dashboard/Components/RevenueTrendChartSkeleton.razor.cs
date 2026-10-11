namespace OrderPoint.Admin.Dashboard.Components;

public sealed partial class RevenueTrendChartSkeleton
{
    private const int YAxisLabelsCount = 5;

    private const int XAxisLabelsCount = 7;

    private static string ContainerStyle => $"height: {RevenueTrendChart.ChartHeight}px;";
}
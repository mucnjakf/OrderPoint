using System.Globalization;
using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Dashboard.Dtos;
using OrderPoint.Admin.Shared.Extensions;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Dashboard.Components;

public sealed partial class RevenueTrendChart
{
    internal const int ChartHeight = 260;

    private const double ViewBoxSize = 1000;

    private const int GridIntervalsCount = 4;

    private const int HourlyLabelStepInHours = 3;

    private const int MaxDailyLabels = 8;

    private const int DotSize = 12;

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public IReadOnlyList<RevenueTrendPointDto> Points { get; set; } = [];

    [Parameter]
    [EditorRequired]
    public bool IsHourly { get; set; }

    private string GradientId { get; } = $"revenue-gradient-{Guid.NewGuid():N}";

    private int? HoveredIndex { get; set; }

    private double AxisMax { get; set; }

    private string LinePath { get; set; } = string.Empty;

    private string AreaPath { get; set; } = string.Empty;

    private IReadOnlyList<string> YAxisLabels { get; set; } = [];

    private IReadOnlyList<string> GridLineYs { get; set; } = [];

    private int XAxisLabelStep { get; set; } = 1;

    private static string ContainerStyle => $"height: {ChartHeight}px;";

    protected override void OnParametersSet()
    {
        HoveredIndex = null;

        double maxRevenue = Points.Count == 0 ? 0 : Points.Max(point => (double)point.Revenue);
        AxisMax = GetNiceAxisMax(maxRevenue);

        YAxisLabels = Enumerable
            .Range(0, GridIntervalsCount + 1)
            .Select(step => (AxisMax * (GridIntervalsCount - step) / GridIntervalsCount)
                .ToString("C0", CultureInfo.CurrentCulture))
            .ToList();

        GridLineYs = Enumerable
            .Range(0, GridIntervalsCount + 1)
            .Select(step => Format(ViewBoxSize * step / GridIntervalsCount))
            .ToList();

        List<(double X, double Y)> coordinates = Points
            .Select((point, index) => (GetXValue(index), GetYValue((double)point.Revenue)))
            .ToList();

        LinePath = BuildMonotonePath(coordinates);
        AreaPath = coordinates.Count == 0
            ? string.Empty
            : $"{LinePath} L {Format(coordinates[^1].X)} {Format(ViewBoxSize)} " +
              $"L {Format(coordinates[0].X)} {Format(ViewBoxSize)} Z";

        XAxisLabelStep = IsHourly ? HourlyLabelStepInHours : Math.Max(1, Points.Count / MaxDailyLabels);
    }

    private string GetX(int index)
    {
        return Format(GetXValue(index));
    }

    private double GetXValue(int index)
    {
        return Points.Count <= 1 ? ViewBoxSize / 2 : index * ViewBoxSize / (Points.Count - 1);
    }

    private double GetYValue(double revenue)
    {
        return AxisMax == 0 ? ViewBoxSize : ViewBoxSize - revenue / AxisMax * ViewBoxSize;
    }

    private string GetDotStyle(int index)
    {
        double leftPercent = GetXValue(index) / ViewBoxSize * 100;
        double topPercent = GetYValue((double)Points[index].Revenue) / ViewBoxSize * 100;

        return $"position: absolute; left: {Format(leftPercent)}%; top: {Format(topPercent)}%; " +
               $"width: {DotSize}px; height: {DotSize}px; border-radius: 50%; transform: translate(-50%, -50%); " +
               "background-color: var(--mud-palette-primary); border: 2px solid var(--mud-palette-surface); " +
               "pointer-events: none;";
    }

    private string GetSlotStyle(int index)
    {
        double slotWidth = Points.Count <= 1 ? 100 : 100.0 / (Points.Count - 1);
        double center = Points.Count <= 1 ? 50 : index * slotWidth;
        double left = Math.Max(0, center - slotWidth / 2);
        double right = Math.Min(100, center + slotWidth / 2);

        return $"position: absolute; top: 0; bottom: 0; left: {Format(left)}%; width: {Format(right - left)}%;";
    }

    private string GetTooltipAnchorStyle(int index)
    {
        double leftPercent = GetXValue(index) / ViewBoxSize * 100;

        return $"position: absolute; top: 0; bottom: 0; left: {Format(leftPercent)}%; width: 1px; " +
               "pointer-events: none;";
    }

    private string GetXAxisLabelStyle(int index)
    {
        double leftPercent = GetXValue(index) / ViewBoxSize * 100;

        return $"position: absolute; top: 4px; left: {Format(leftPercent)}%; transform: translateX(-50%); " +
               "white-space: nowrap;";
    }

    private string GetAxisLabel(DateTimeOffset bucketStartUtc)
    {
        DateTimeOffset bucketStart = bucketStartUtc.ToTimeZone(TimeZoneService.TimeZone);
        CultureInfo culture = CultureInfo.CurrentCulture;

        return IsHourly
            ? bucketStart.ToString("t", culture)
            : bucketStart.ToString(culture.GetShortMonthDayPattern(), culture);
    }

    private string GetTooltipLabel(DateTimeOffset bucketStartUtc)
    {
        DateTimeOffset bucketStart = bucketStartUtc.ToTimeZone(TimeZoneService.TimeZone);
        CultureInfo culture = CultureInfo.CurrentCulture;

        return IsHourly
            ? $"{bucketStart.ToString("t", culture)} – {bucketStart.AddHours(1).ToString("t", culture)}"
            : bucketStart.ToString($"dddd, {culture.DateTimeFormat.MonthDayPattern}", culture);
    }

    private static double GetNiceAxisMax(double maxValue)
    {
        if (maxValue <= 0)
        {
            return 1;
        }

        double magnitude = Math.Pow(10, Math.Floor(Math.Log10(maxValue)));
        double normalized = maxValue / magnitude;

        double niceNormalized = normalized switch
        {
            <= 1 => 1,
            <= 2 => 2,
            <= 2.5 => 2.5,
            <= 5 => 5,
            _ => 10
        };

        return niceNormalized * magnitude;
    }

    private static string BuildMonotonePath(IReadOnlyList<(double X, double Y)> points)
    {
        if (points.Count == 0)
        {
            return string.Empty;
        }

        if (points.Count == 1)
        {
            return $"M {Format(points[0].X)} {Format(points[0].Y)}";
        }

        int count = points.Count;
        var slopes = new double[count - 1];
        var tangents = new double[count];

        for (int i = 0; i < count - 1; i++)
        {
            slopes[i] = (points[i + 1].Y - points[i].Y) / (points[i + 1].X - points[i].X);
        }

        tangents[0] = slopes[0];
        tangents[count - 1] = slopes[count - 2];

        for (int i = 1; i < count - 1; i++)
        {
            tangents[i] = slopes[i - 1] * slopes[i] <= 0 ? 0 : (slopes[i - 1] + slopes[i]) / 2;
        }

        var path = new System.Text.StringBuilder($"M {Format(points[0].X)} {Format(points[0].Y)}");

        for (int i = 0; i < count - 1; i++)
        {
            double third = (points[i + 1].X - points[i].X) / 3;

            path.Append($" C {Format(points[i].X + third)} {Format(points[i].Y + tangents[i] * third)}");
            path.Append($" {Format(points[i + 1].X - third)} {Format(points[i + 1].Y - tangents[i + 1] * third)}");
            path.Append($" {Format(points[i + 1].X)} {Format(points[i + 1].Y)}");
        }

        return path.ToString();
    }

    private static string Format(double value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }
}
using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Dashboard.Dtos;

namespace OrderPoint.Admin.Dashboard.Components;

public sealed partial class RevenueTrendChart
{
    private const string ChartHeight = "280px";

    private const int HourlyLabelStepInHours = 3;

    private const int MaxDailyLabels = 8;

    [Parameter]
    [EditorRequired]
    public IReadOnlyList<RevenueTrendPointDto> Points { get; set; } = [];

    [Parameter]
    [EditorRequired]
    public bool IsHourly { get; set; }

    private List<ChartSeries<double>> Series { get; set; } = [];

    private TimeSeriesChartOptions Options { get; set; } = new();

    protected override void OnParametersSet()
    {
        Series =
        [
            new ChartSeries<double>
            {
                Name = "Revenue",
                Data = Points
                    .Select(point => (point.BucketStartUtc.UtcDateTime, (double)point.Revenue))
                    .ToArray(),
                TooltipYValueFormat = "C"
            }
        ];

        int dailyLabelStep = Math.Max(1, Points.Count / MaxDailyLabels);

        Options = new TimeSeriesChartOptions
        {
            LineDisplayType = LineDisplayType.Area,
            InterpolationOption = InterpolationOption.NaturalSpline,
            ShowLegend = false,
            YAxisRequireZeroPoint = true,
            YAxisToStringFunc = value => value.ToString("C0", CultureInfo.CurrentCulture),
            TimeLabelFormat = IsHourly ? "HH:mm" : "dd MMM",
            TooltipTimeLabelFormat = IsHourly ? "HH:mm" : "ddd, dd MMM",
            TimeLabelSpacing = IsHourly
                ? TimeSpan.FromHours(HourlyLabelStepInHours)
                : TimeSpan.FromDays(dailyLabelStep),
            TooltipTitleFormat = "{{X_VALUE}}",
            TooltipSubtitleFormat = "{{Y_VALUE}}"
        };
    }
}
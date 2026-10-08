using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Dashboard.Dtos;

namespace OrderPoint.Admin.Dashboard.Components;

public sealed partial class RevenueTrendChart
{
    private const string ChartHeight = "280px";

    private const int MaxVisibleLabels = 8;

    private static readonly LineChartOptions Options = new()
    {
        LineDisplayType = LineDisplayType.Area,
        InterpolationOption = InterpolationOption.NaturalSpline,
        ShowLegend = false,
        YAxisRequireZeroPoint = true,
        YAxisToStringFunc = value => value.ToString("C0", CultureInfo.CurrentCulture)
    };

    [Parameter]
    [EditorRequired]
    public IReadOnlyList<RevenueTrendPointDto> Points { get; set; } = [];

    [Parameter]
    [EditorRequired]
    public bool IsHourly { get; set; }

    private List<ChartSeries<double>> Series { get; set; } = [];

    private string[] Labels { get; set; } = [];

    protected override void OnParametersSet()
    {
        Series =
        [
            new ChartSeries<double>
            {
                Name = "Revenue",
                Data = Points.Select(point => (double)point.Revenue).ToArray()
            }
        ];

        int labelStep = Math.Max(1, Points.Count / MaxVisibleLabels);

        Labels = Points
            .Select((point, index) => index % labelStep == 0 ? GetLabel(point.BucketStartUtc) : string.Empty)
            .ToArray();
    }

    private string GetLabel(DateTimeOffset bucketStartUtc)
    {
        return IsHourly
            ? bucketStartUtc.ToString("HH:mm", CultureInfo.CurrentCulture)
            : bucketStartUtc.ToString("dd MMM", CultureInfo.CurrentCulture);
    }
}
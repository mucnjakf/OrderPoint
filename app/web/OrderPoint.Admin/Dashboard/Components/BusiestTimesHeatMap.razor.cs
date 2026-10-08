using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Dashboard.Dtos;

namespace OrderPoint.Admin.Dashboard.Components;

public sealed partial class BusiestTimesHeatMap
{
    private const string ChartHeight = "280px";

    private const int HoursPerDay = 24;

    private static readonly DayOfWeek[] WeekDays =
    [
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday,
        DayOfWeek.Saturday,
        DayOfWeek.Sunday
    ];

    private static readonly HeatMapChartOptions Options = new()
    {
        ShowLegend = false,
        ShowLabels = false,
        EnableSmoothGradient = true,
        ShowToolTips = true,
        ValueFormatString = "F0",
        TooltipTitleFormat = "{{SERIES_NAME}} {{X_VALUE}}:00",
        TooltipSubtitleFormat = "{{Y_VALUE}} orders"
    };

    [Parameter]
    [EditorRequired]
    public IReadOnlyList<BusiestTimeDto> BusiestTimes { get; set; } = [];

    private List<ChartSeries<double>> Series { get; set; } = [];

    private string[] Labels { get; set; } = [];

    protected override void OnParametersSet()
    {
        Dictionary<(DayOfWeek, int), int> ordersCountBySlot = BusiestTimes
            .ToDictionary(slot => (slot.DayOfWeek, slot.Hour), slot => slot.OrdersCount);

        Series = WeekDays
            .Select(dayOfWeek => new ChartSeries<double>
            {
                Name = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedDayName(dayOfWeek),
                Data = Enumerable
                    .Range(0, HoursPerDay)
                    .Select(hour => (double)ordersCountBySlot.GetValueOrDefault((dayOfWeek, hour)))
                    .ToArray()
            })
            .ToList();

        Labels = Enumerable
            .Range(0, HoursPerDay)
            .Select(hour => $"{hour:00}")
            .ToArray();
    }
}
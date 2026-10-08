using System.Globalization;
using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Dashboard.Dtos;

namespace OrderPoint.Admin.Dashboard.Components;

public sealed partial class BusiestTimesHeatMap
{
    private const int HoursPerDay = 24;

    private const int LabelHourStep = 3;

    private const int CellHeight = 24;

    private const double MinOpacity = 0.06;

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

    [Parameter]
    [EditorRequired]
    public IReadOnlyList<BusiestTimeDto> BusiestTimes { get; set; } = [];

    private Dictionary<(DayOfWeek, int), int> OrdersCountBySlot { get; set; } = [];

    private int MaxOrdersCount { get; set; }

    private static string DayLabelStyle => "width: 36px; flex-shrink: 0;";

    protected override void OnParametersSet()
    {
        OrdersCountBySlot = BusiestTimes.ToDictionary(slot => (slot.DayOfWeek, slot.Hour), slot => slot.OrdersCount);
        MaxOrdersCount = BusiestTimes.Count == 0 ? 0 : BusiestTimes.Max(slot => slot.OrdersCount);
    }

    private int GetOrdersCount(DayOfWeek dayOfWeek, int hour)
    {
        return OrdersCountBySlot.GetValueOrDefault((dayOfWeek, hour));
    }

    private string GetCellStyle(int ordersCount)
    {
        double opacity = MaxOrdersCount == 0
            ? MinOpacity
            : MinOpacity + (1 - MinOpacity) * ordersCount / MaxOrdersCount;

        string formattedOpacity = opacity.ToString("0.##", CultureInfo.InvariantCulture);

        return $"height: {CellHeight}px; border-radius: 4px; " +
               $"background-color: rgba(var(--mud-palette-primary-rgb), {formattedOpacity});";
    }

    private static string GetDayLabel(DayOfWeek dayOfWeek)
    {
        return CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedDayName(dayOfWeek);
    }

    private static string GetDayName(DayOfWeek dayOfWeek)
    {
        return CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(dayOfWeek);
    }
}
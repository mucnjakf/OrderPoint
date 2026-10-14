using System.Globalization;
using Microsoft.AspNetCore.Components;
using OrderPoint.Bar.Shift.Dtos;
using OrderPoint.Shared.Extensions;

namespace OrderPoint.Bar.Shift.Components;

// One bar per local hour; phones have no hover, so the counts sit above the bars
public sealed partial class ShiftHoursChart
{
    private const int MaxBarHeight = 96;

    private const int EmptyBarHeight = 4;

    private const string BarsStyle = "display: flex; align-items: flex-end; gap: 4px; height: 120px;";

    private const string ColumnStyle =
        "flex: 1; min-width: 0; display: flex; flex-direction: column; align-items: center; justify-content: flex-end;";

    // Centred labels overflow to both sides, into the unlabelled slots next to them
    private const string LabelsStyle = "display: flex; gap: 4px;";

    private const string LabelStyle =
        "flex: 1; min-width: 0; display: flex; justify-content: center; white-space: nowrap;";

    [Parameter]
    [EditorRequired]
    public IReadOnlyList<ShiftHourDto> Hours { get; set; } = [];

    private int MaxCompletedCount => Hours.Count == 0 ? 0 : Hours.Max(hour => hour.CompletedCount);

    // Every hour is labelled on a short shift, every second or third one on a long shift
    private int LabelStep => Hours.Count switch
    {
        <= 8 => 1,
        <= 16 => 2,
        _ => 3
    };

    private string GetBarStyle(ShiftHourDto hour)
    {
        double height = hour.CompletedCount == 0 || MaxCompletedCount == 0
            ? EmptyBarHeight
            : hour.CompletedCount * MaxBarHeight / (double)MaxCompletedCount;

        string background = hour.CompletedCount == 0
            ? "rgba(var(--mud-palette-text-primary-rgb), 0.08)"
            : "var(--mud-palette-primary)";

        string heightText = height.ToString("0.#", CultureInfo.InvariantCulture);

        return $"width: 100%; height: {heightText}px; background: {background}; border-radius: 4px 4px 0 0; " +
            "transition: height 300ms ease;";
    }

    private static string GetHourLabel(int hour)
    {
        CultureInfo culture = CultureInfo.CurrentCulture;

        return DateTime.MinValue.AddHours(hour).ToString(culture.Uses12HourClock() ? "h tt" : "HH", culture);
    }
}
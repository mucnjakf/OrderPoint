using System.Globalization;
using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Dashboard.Dtos;

namespace OrderPoint.Admin.Dashboard.Components;

public sealed partial class CategoryRevenueDonut
{
    private const int DonutSize = 170;

    private const double Radius = 40;

    private const double StrokeWidth = 14;

    private const double HoveredStrokeWidth = 18;

    private const double SegmentGap = 0.8;

    private const string HoveredRowStyle = "background-color: var(--mud-palette-table-hover);";

    private static readonly string[] SegmentColors =
    [
        "var(--mud-palette-primary)",
        "var(--mud-palette-info)",
        "var(--mud-palette-success)",
        "var(--mud-palette-warning)",
        "var(--mud-palette-secondary)"
    ];

    [Parameter]
    [EditorRequired]
    public IReadOnlyList<CategoryRevenueDto> CategoryRevenue { get; set; } = [];

    private IReadOnlyList<DonutSegment> Segments { get; set; } = [];

    private decimal TotalRevenue { get; set; }

    private int? HoveredIndex { get; set; }

    private static string DonutContainerStyle =>
        $"position: relative; width: {DonutSize}px; height: {DonutSize}px; flex-shrink: 0;";

    private static string CenterStyle => "position: absolute; inset: 22%; pointer-events: none;";

    private string CenterLabel => HoveredIndex is null ? "Total" : CategoryRevenue[HoveredIndex.Value].CategoryName;

    private string CenterValue => (HoveredIndex is null ? TotalRevenue : CategoryRevenue[HoveredIndex.Value].Revenue)
        .ToString("C", CultureInfo.CurrentCulture);

    protected override void OnParametersSet()
    {
        HoveredIndex = null;
        TotalRevenue = CategoryRevenue.Sum(category => category.Revenue);

        double circumference = 2 * Math.PI * Radius;
        double offset = 0;
        var segments = new List<DonutSegment>();

        foreach (CategoryRevenueDto category in CategoryRevenue)
        {
            double share = TotalRevenue == 0 ? 0 : (double)(category.Revenue / TotalRevenue);
            double length = Math.Max(0, share * circumference - SegmentGap);

            segments.Add(new DonutSegment(
                $"{Format(length)} {Format(circumference - length)}",
                Format(-offset),
                $"{share * 100:0}% of top categories"));

            offset += share * circumference;
        }

        Segments = segments;
    }

    private string GetSegmentStyle(int index)
    {
        return $"stroke: {SegmentColors[index % SegmentColors.Length]}; cursor: pointer; " +
               "transition: stroke-width 0.15s ease-in-out;";
    }

    private string? GetLegendRowStyle(int index)
    {
        return HoveredIndex == index ? HoveredRowStyle : null;
    }

    private static string GetLegendDotStyle(int index)
    {
        return "width: 10px; height: 10px; border-radius: 50%; flex-shrink: 0; " +
               $"background-color: {SegmentColors[index % SegmentColors.Length]};";
    }

    private static string Format(double value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private sealed record DonutSegment(string DashArray, string DashOffset, string ShareText);
}
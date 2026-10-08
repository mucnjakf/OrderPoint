using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace OrderPoint.Admin.Dashboard.Components;

public sealed partial class DashboardKpiCard
{
    [Parameter]
    [EditorRequired]
    public string Label { get; set; }

    [Parameter]
    [EditorRequired]
    public string Icon { get; set; }

    [Parameter]
    [EditorRequired]
    public Color Color { get; set; }

    [Parameter]
    [EditorRequired]
    public string Value { get; set; }

    [Parameter]
    [EditorRequired]
    public double? CurrentValue { get; set; }

    [Parameter]
    [EditorRequired]
    public double? PreviousValue { get; set; }

    [Parameter]
    public bool IsLowerBetter { get; set; }

    [Parameter]
    [EditorRequired]
    public bool IsLoading { get; set; }

    private double? ChangePercent => CurrentValue is null || PreviousValue is null or 0
        ? null
        : (CurrentValue.Value - PreviousValue.Value) / PreviousValue.Value * 100;

    private bool IsImprovement => IsLowerBetter ? ChangePercent < 0 : ChangePercent > 0;

    private Color ChangeColor => ChangePercent is null or 0
        ? Color.Default
        : IsImprovement ? Color.Success : Color.Error;

    private string ChangeIcon => ChangePercent switch
    {
        > 0 => Icons.Material.Filled.TrendingUp,
        < 0 => Icons.Material.Filled.TrendingDown,
        _ => Icons.Material.Filled.TrendingFlat
    };

    private string ChangeText => ChangePercent is null ? "No data" : $"{Math.Abs(ChangePercent.Value):0}%";
}
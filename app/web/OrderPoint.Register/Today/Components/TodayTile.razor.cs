using Microsoft.AspNetCore.Components;

namespace OrderPoint.Register.Today.Components;

// One of the day's totals, in euros
public sealed partial class TodayTile
{
    [Parameter]
    [EditorRequired]
    public string Icon { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public string Title { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public decimal Value { get; set; }

    [Parameter]
    [EditorRequired]
    public string Caption { get; set; } = null!;
}
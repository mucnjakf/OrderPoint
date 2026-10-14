using Microsoft.AspNetCore.Components;

namespace OrderPoint.Bar.Orders.Components;

// A small pill next to a tab's name; nothing while the count is loading
public sealed partial class TabCount
{
    private const string CountStyle =
        "margin-left: 8px; padding: 0 8px; border-radius: 10px; font-size: 12px; " +
        "background-color: rgba(var(--mud-palette-text-primary-rgb), 0.08);";

    [Parameter]
    [EditorRequired]
    public int? Count { get; set; }
}
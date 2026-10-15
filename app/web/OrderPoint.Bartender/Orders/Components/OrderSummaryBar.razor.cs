using Microsoft.AspNetCore.Components;
using OrderPoint.Bartender.Shared.Layout;

namespace OrderPoint.Bartender.Orders.Components;

// What the order has so far, above the bottom navigation while the list scrolls under it
public sealed partial class OrderSummaryBar
{
    private const string BarStyle = $"position: sticky; bottom: calc({BottomNavigation.Height} + 12px); z-index: 1;";

    [Parameter]
    [EditorRequired]
    public int ItemsCount { get; set; }

    [Parameter]
    [EditorRequired]
    public decimal Total { get; set; }

    [Parameter]
    [EditorRequired]
    public string ReviewUri { get; set; } = null!;
}
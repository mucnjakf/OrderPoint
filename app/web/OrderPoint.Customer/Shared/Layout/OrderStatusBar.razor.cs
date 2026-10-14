using Microsoft.AspNetCore.Components;
using OrderPoint.Customer.Orders.Dtos;
using OrderPoint.Customer.Shared.Extensions;
using OrderPoint.Shared.Orders.Enumerations;

namespace OrderPoint.Customer.Shared.Layout;

// Fixed right above the bottom navigation: how the guest's latest order is doing, on every page
public sealed partial class OrderStatusBar
{
    internal const string Height = "48px";

    private const string BarStyle =
        $"position: fixed; left: 0; right: 0; bottom: {BottomNavigation.Height}; height: {Height}; " +
        "display: flex; align-items: center; gap: 12px; padding: 0 16px; z-index: var(--mud-zindex-appbar); " +
        "color: inherit; text-decoration: none; background-color: var(--mud-palette-surface); " +
        "border-top: 1px solid var(--mud-palette-lines-default);";

    private const string TextStyle = "min-width: 0; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;";

    [Parameter]
    [EditorRequired]
    public OrderTrackingDto Order { get; set; } = null!;

    // The guest's other open orders; with any, the bar opens My orders instead of this one
    [Parameter]
    public int OtherOpenCount { get; set; }

    private string Href => OtherOpenCount > 0 ? "/orders" : $"/orders/{Order.Id}";

    private string StatusText => Order.Status is OrderStatus.Declined && Order.DeclineReason is not null
        ? $"{Order.Status.GetGuestTitle()} · {Order.DeclineReason}"
        : Order.Status.GetGuestTitle();
}
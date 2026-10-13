using OrderPoint.Bartender.Orders.Enumerations;

namespace OrderPoint.Bartender.Orders.Services;

// One per circuit: the Orders page opens on the tab the bartender left it on
internal sealed class OrdersTabState
{
    internal OrdersTab Tab { get; set; }
}
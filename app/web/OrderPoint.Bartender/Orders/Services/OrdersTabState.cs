using OrderPoint.Bartender.Orders.Enumerations;

namespace OrderPoint.Bartender.Orders.Services;

// One per circuit: the Orders page opens on the tab the bartender left it on, and on a tablet with the order that was
// open in the panel (e.g. after returning from the decline page)
internal sealed class OrdersTabState
{
    internal OrdersTab Tab { get; set; }

    internal Guid? SelectedOrderId { get; set; }
}
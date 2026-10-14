namespace OrderPoint.Application.Notifications;

// One per request: the orders a command changed, collected by its handler (a release can change several, known only
// once it ran), so OrderNotificationBehavior can tell the guests following them
internal sealed class ChangedOrders
{
    private readonly List<Guid> _orderIds = [];

    internal IReadOnlyList<Guid> OrderIds => _orderIds;

    internal void Add(Guid orderId)
    {
        _orderIds.Add(orderId);
    }
}
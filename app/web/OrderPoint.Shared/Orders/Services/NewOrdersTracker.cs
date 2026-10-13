using OrderPoint.Shared.Orders.Dtos;

namespace OrderPoint.Shared.Orders.Services;

// One per list; orders created after the previous load started are new to the viewer for a few seconds
internal sealed class NewOrdersTracker
{
    private static readonly TimeSpan NewDuration = TimeSpan.FromSeconds(10);

    private readonly HashSet<Guid> _newOrderIds = [];

    private DateTimeOffset? _lastLoadStartedAtUtc;

    internal bool IsNew(Guid orderId)
    {
        return _newOrderIds.Contains(orderId);
    }

    internal void Track(IReadOnlyList<OrderDto> orders, DateTimeOffset loadStartedAtUtc, Func<Task> onExpiredAsync)
    {
        DateTimeOffset? previousLoadStartedAtUtc = _lastLoadStartedAtUtc;

        // Parallel loads can finish out of order
        _lastLoadStartedAtUtc = previousLoadStartedAtUtc > loadStartedAtUtc
            ? previousLoadStartedAtUtc
            : loadStartedAtUtc;

        if (previousLoadStartedAtUtc is null)
        {
            return;
        }

        List<Guid> newOrderIds = orders
            .Where(order => order.CreatedAtUtc > previousLoadStartedAtUtc && _newOrderIds.Add(order.Id))
            .Select(order => order.Id)
            .ToList();

        if (newOrderIds.Count == 0)
        {
            return;
        }

        _ = ExpireAsync(newOrderIds, onExpiredAsync);
    }

    private async Task ExpireAsync(List<Guid> orderIds, Func<Task> onExpiredAsync)
    {
        await Task.Delay(NewDuration);

        _newOrderIds.ExceptWith(orderIds);

        await onExpiredAsync();
    }
}
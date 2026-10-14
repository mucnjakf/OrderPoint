namespace OrderPoint.Application.Notifications;

public interface IOrderNotifier
{
    // Tells the staff apps, and the guests following any of the changed orders
    Task NotifyOrdersChangedAsync(IReadOnlyList<Guid> changedOrderIds, CancellationToken cancellationToken = default);
}
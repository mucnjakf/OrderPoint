namespace OrderPoint.Application.Notifications;

public interface IOrderNotifier
{
    // Tells the staff apps, and the guest following the order when there is one
    Task NotifyOrdersChangedAsync(Guid? trackedOrderId, CancellationToken cancellationToken = default);
}
namespace OrderPoint.Application.Notifications;

public interface IOrderNotifier
{
    Task NotifyOrdersChangedAsync(CancellationToken cancellationToken = default);
}
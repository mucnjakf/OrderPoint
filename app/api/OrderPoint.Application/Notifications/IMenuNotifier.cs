namespace OrderPoint.Application.Notifications;

public interface IMenuNotifier
{
    // Tells the staff apps and every guest, since anyone may be looking at the menu
    Task NotifyMenuChangedAsync(CancellationToken cancellationToken = default);
}
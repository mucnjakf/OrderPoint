namespace OrderPoint.Application.Notifications;

public interface ITablesNotifier
{
    // Tells the staff apps (floor plan) and every guest (their table)
    Task NotifyTablesChangedAsync(CancellationToken cancellationToken = default);
}
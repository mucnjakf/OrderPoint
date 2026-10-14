using Microsoft.AspNetCore.SignalR;
using OrderPoint.Application.Notifications;

namespace OrderPoint.Api.Hubs;

internal sealed partial class SignalRTablesNotifier(
    IHubContext<OrdersHub> ordersHubContext,
    IHubContext<OrderTrackingHub> orderTrackingHubContext,
    ILogger<SignalRTablesNotifier> logger)
    : ITablesNotifier
{
    public async Task NotifyTablesChangedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.WhenAll(
                ordersHubContext.Clients.All.SendAsync(OrdersHub.TablesChangedMethod, cancellationToken),
                orderTrackingHubContext.Clients.All.SendAsync(OrderTrackingHub.TablesChangedMethod, cancellationToken));
        }
        catch (Exception exception)
        {
            // The change is already saved; the apps catch up on their next reload
            LogNotificationFailed(exception);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Could not tell the apps that the tables changed")]
    private partial void LogNotificationFailed(Exception exception);
}
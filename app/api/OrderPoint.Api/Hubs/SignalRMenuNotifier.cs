using Microsoft.AspNetCore.SignalR;
using OrderPoint.Application.Notifications;

namespace OrderPoint.Api.Hubs;

internal sealed partial class SignalRMenuNotifier(
    IHubContext<OrdersHub> ordersHubContext,
    IHubContext<OrderTrackingHub> orderTrackingHubContext,
    ILogger<SignalRMenuNotifier> logger)
    : IMenuNotifier
{
    public async Task NotifyMenuChangedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.WhenAll(
                ordersHubContext.Clients.All.SendAsync(OrdersHub.MenuChangedMethod, cancellationToken),
                orderTrackingHubContext.Clients.All.SendAsync(OrderTrackingHub.MenuChangedMethod, cancellationToken));
        }
        catch (Exception exception)
        {
            // The change is already saved; the apps catch up on their next reload
            LogNotificationFailed(exception);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Could not tell the apps that the menu changed")]
    private partial void LogNotificationFailed(Exception exception);
}
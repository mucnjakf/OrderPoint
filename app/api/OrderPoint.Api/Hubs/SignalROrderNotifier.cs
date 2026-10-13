using Microsoft.AspNetCore.SignalR;
using OrderPoint.Application.Notifications;

namespace OrderPoint.Api.Hubs;

internal sealed partial class SignalROrderNotifier(
    IHubContext<OrdersHub> hubContext,
    ILogger<SignalROrderNotifier> logger)
    : IOrderNotifier
{
    public async Task NotifyOrdersChangedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await hubContext.Clients.All.SendAsync(OrdersHub.OrdersChangedMethod, cancellationToken);
        }
        catch (Exception exception)
        {
            // The change is already saved; the apps catch up on their next reload
            LogNotificationFailed(exception);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Could not tell the apps that orders changed")]
    private partial void LogNotificationFailed(Exception exception);
}
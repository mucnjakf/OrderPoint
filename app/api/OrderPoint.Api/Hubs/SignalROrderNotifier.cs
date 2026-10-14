using Microsoft.AspNetCore.SignalR;
using OrderPoint.Application.Notifications;

namespace OrderPoint.Api.Hubs;

internal sealed partial class SignalROrderNotifier(
    IHubContext<OrdersHub> ordersHubContext,
    IHubContext<OrderTrackingHub> orderTrackingHubContext,
    ILogger<SignalROrderNotifier> logger)
    : IOrderNotifier
{
    public async Task NotifyOrdersChangedAsync(
        IReadOnlyList<Guid> changedOrderIds,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await ordersHubContext.Clients.All.SendAsync(OrdersHub.OrdersChangedMethod, cancellationToken);

            if (changedOrderIds.Count != 0)
            {
                await orderTrackingHubContext.Clients
                    .Groups(changedOrderIds.Select(OrderTrackingHub.GetGroupName).ToList())
                    .SendAsync(OrderTrackingHub.OrderChangedMethod, cancellationToken);
            }
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
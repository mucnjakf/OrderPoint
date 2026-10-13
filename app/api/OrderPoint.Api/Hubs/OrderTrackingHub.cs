using Microsoft.AspNetCore.SignalR;

namespace OrderPoint.Api.Hubs;

// Anonymous, for the customer app: a connection asks to follow an order by its id (as unguessable as the tracking
// endpoint's) and then hears OrderChanged, without arguments, whenever that order changes status
internal sealed class OrderTrackingHub : Hub
{
    internal const string TrackOrderMethod = "TrackOrder";

    internal const string OrderChangedMethod = "OrderChanged";

    // Groups don't survive a reconnect, so clients call this again after one
    [HubMethodName(TrackOrderMethod)]
    public async Task TrackOrderAsync(Guid orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(orderId));
    }

    internal static string GetGroupName(Guid orderId) => $"order:{orderId}";
}
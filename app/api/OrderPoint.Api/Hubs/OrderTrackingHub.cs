using Microsoft.AspNetCore.SignalR;

namespace OrderPoint.Api.Hubs;

// Anonymous, for the customer app: a connection asks to follow an order by its id (as unguessable as the tracking
// endpoint's) and then hears OrderChanged, without arguments, whenever that order changes status
internal sealed class OrderTrackingHub : Hub
{
    internal const string TrackOrderMethod = "TrackOrder";

    internal const string OrderChangedMethod = "OrderChanged";

    // Sent to every connection after an item or a category changes, so the menu a guest sees stays current
    internal const string MenuChangedMethod = "MenuChanged";

    // Sent to every connection after an area, a table or a fixture changes
    internal const string TablesChangedMethod = "TablesChanged";

    // Groups don't survive a reconnect, so clients call this again after one
    [HubMethodName(TrackOrderMethod)]
    public async Task TrackOrderAsync(Guid orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(orderId));
    }

    internal static string GetGroupName(Guid orderId) => $"order:{orderId}";
}
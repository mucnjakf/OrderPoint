using Microsoft.AspNetCore.SignalR;

namespace OrderPoint.Api.Hubs;

// Clients only listen: the API sends OrdersChanged when orders change and MenuChanged when the menu does
internal sealed class OrdersHub : Hub
{
    internal const string OrdersChangedMethod = "OrdersChanged";

    internal const string MenuChangedMethod = "MenuChanged";
}
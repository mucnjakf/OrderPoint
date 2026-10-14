using Microsoft.AspNetCore.SignalR;

namespace OrderPoint.Api.Hubs;

// Clients only listen: the API sends OrdersChanged, MenuChanged and TablesChanged when those change
internal sealed class OrdersHub : Hub
{
    internal const string OrdersChangedMethod = "OrdersChanged";

    internal const string MenuChangedMethod = "MenuChanged";

    internal const string TablesChangedMethod = "TablesChanged";
}
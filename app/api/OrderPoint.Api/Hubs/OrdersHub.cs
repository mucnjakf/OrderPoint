using Microsoft.AspNetCore.SignalR;

namespace OrderPoint.Api.Hubs;

// Clients only listen: the API sends OrdersChanged after an order is placed or changes status
internal sealed class OrdersHub : Hub
{
    internal const string OrdersChangedMethod = "OrdersChanged";
}
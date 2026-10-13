using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Admin.Orders.Api;
using OrderPoint.Shared.Orders.Enumerations;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Orders.Dialogs;

public sealed partial class OrderDetailsDialog : IDisposable
{
    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private OrderApiClient OrderApiClient { get; set; } = null!;

    [Inject]
    private OrdersHubService OrdersHubService { get; set; } = null!;

    [Parameter]
    public OrderDto Order { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    // The parameter is reset whenever the dialog provider re-renders, so live updates go here
    private OrderDto CurrentOrder { get; set; } = null!;

    private IReadOnlyList<(OrderStatus Status, DateTimeOffset? ReachedAtUtc)> TimelineSteps =>
        CurrentOrder.Status is OrderStatus.Declined
            ?
            [
                (OrderStatus.Pending, CurrentOrder.CreatedAtUtc),
                (OrderStatus.Declined, CurrentOrder.DeclinedAtUtc)
            ]
            :
            [
                (OrderStatus.Pending, CurrentOrder.CreatedAtUtc),
                (OrderStatus.Accepted, CurrentOrder.AcceptedAtUtc),
                (OrderStatus.Active, CurrentOrder.ActivatedAtUtc),
                (OrderStatus.Completed, CurrentOrder.CompletedAtUtc)
            ];

    protected override void OnInitialized()
    {
        CurrentOrder = Order;

        OrdersHubService.OrdersChanged += OnOrdersChanged;
    }

    public void Dispose()
    {
        OrdersHubService.OrdersChanged -= OnOrdersChanged;
    }

    private void OnOrdersChanged()
    {
        _ = InvokeAsync(GetOrderAsync);
    }

    private async Task GetOrderAsync()
    {
        OrderDto? order = await ApiService.ExecuteAsync(() => OrderApiClient.GetOrderAsync(Order.Id));

        if (order is null)
        {
            return;
        }

        CurrentOrder = order;

        StateHasChanged();
    }

    private static decimal GetSubtotal(OrderItemDto orderItem)
    {
        return orderItem.Quantity * orderItem.UnitPrice;
    }

    private void Close()
    {
        MudDialogInstance.Close(DialogResult.Ok(true));
    }
}
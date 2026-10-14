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

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Parameter]
    public OrderDto Order { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    // The parameter is reset whenever the dialog provider re-renders, so live updates go here
    private OrderDto CurrentOrder { get; set; } = null!;

    private bool IsDeclining { get; set; }

    // Pending, accepted or active: the admin can still decline it
    private bool IsOpen => CurrentOrder.Status is OrderStatus.Pending or OrderStatus.Accepted or OrderStatus.Active;

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

    private async Task ShowDeclineOrderDialogAsync()
    {
        var parameters = new DialogParameters<DeclineOrderDialog>
        {
            { dialog => dialog.Order, CurrentOrder }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<DeclineOrderDialog>(string.Empty, parameters, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        IsDeclining = true;

        bool isSuccess = await ApiService.ExecuteAsync(
            () => OrderApiClient.DeclineOrderAsync(CurrentOrder.Id),
            $"Order #{CurrentOrder.Number} declined");

        IsDeclining = false;

        if (isSuccess)
        {
            await GetOrderAsync();
        }
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
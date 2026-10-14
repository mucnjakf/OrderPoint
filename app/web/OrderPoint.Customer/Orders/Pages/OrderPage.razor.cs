using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Customer.Orders.Api;
using OrderPoint.Customer.Orders.Dialogs;
using OrderPoint.Customer.Orders.Dtos;
using OrderPoint.Customer.Orders.Services;
using OrderPoint.Shared.Services;

namespace OrderPoint.Customer.Orders.Pages;

// Follows one order live: the tracking hub says when it changed
public sealed partial class OrderPage : IDisposable
{
    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private OrderApiClient OrderApiClient { get; set; } = null!;

    [Inject]
    private OrderTrackingHubService OrderTrackingHubService { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Parameter]
    public Guid OrderId { get; set; }

    private OrderTrackingDto? Order { get; set; }

    private bool IsLoading { get; set; } = true;

    private bool IsCancelling { get; set; }

    protected override async Task OnInitializedAsync()
    {
        OrderTrackingHubService.OrdersChanged += OnOrdersChanged;

        // Also follows an order opened from a link, not only the ones placed on this phone
        await OrderTrackingHubService.TrackAsync(OrderId);

        await GetOrderAsync();
    }

    public void Dispose()
    {
        OrderTrackingHubService.OrdersChanged -= OnOrdersChanged;
    }

    private void OnOrdersChanged()
    {
        _ = InvokeAsync(GetOrderAsync);
    }

    // A failed reload keeps what is shown
    private async Task GetOrderAsync()
    {
        OrderTrackingDto? order = await ApiService.ExecuteAsync(() => OrderApiClient.GetOrderTrackingAsync(OrderId));

        if (order is not null || IsLoading)
        {
            Order = order;
        }

        IsLoading = false;

        StateHasChanged();
    }

    // Refused when a bartender took the order first; the reload then shows what happened
    private async Task ShowCancelOrderDialogAsync()
    {
        int orderNumber = Order!.Number;

        var parameters = new DialogParameters<CancelOrderDialog>
        {
            { dialog => dialog.OrderNumber, orderNumber }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.ExtraSmall,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<CancelOrderDialog>(string.Empty, parameters, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        IsCancelling = true;

        await ApiService.ExecuteAsync(
            () => OrderApiClient.CancelOrderAsync(OrderId),
            $"Order #{orderNumber} cancelled");

        IsCancelling = false;

        await GetOrderAsync();
    }
}
using Microsoft.AspNetCore.Components;
using OrderPoint.Customer.Orders.Api;
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

    [Parameter]
    public Guid OrderId { get; set; }

    private OrderTrackingDto? Order { get; set; }

    private bool IsLoading { get; set; } = true;

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
}
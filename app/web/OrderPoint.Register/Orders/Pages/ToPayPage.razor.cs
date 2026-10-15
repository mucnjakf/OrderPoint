using Microsoft.AspNetCore.Components;
using OrderPoint.Register.Orders.Api;
using OrderPoint.Register.Orders.Api.Requests;
using OrderPoint.Register.Orders.Services;
using OrderPoint.Register.Orders.Sorting;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Services;

namespace OrderPoint.Register.Orders.Pages;

// The guests who still have to pay, live, oldest first
public sealed partial class ToPayPage : IDisposable
{
    // Every unpaid order of a day fits on one page
    private const int PageSize = 100;

    private const int LoadingCardsCount = 3;

    // "12 min ago" moves on even when nothing changes
    private const int TimeRefreshIntervalInSeconds = 30;

    private readonly PeriodicTimer _timeRefreshTimer = new(TimeSpan.FromSeconds(TimeRefreshIntervalInSeconds));

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private OrderApiClient OrderApiClient { get; set; } = null!;

    [Inject]
    private OrdersHubService OrdersHubService { get; set; } = null!;

    [Inject]
    private PaymentDialogService PaymentDialogService { get; set; } = null!;

    private IReadOnlyList<OrderDto> Orders { get; set; } = [];

    private bool IsLoading { get; set; } = true;

    private Guid? PayingOrderId { get; set; }

    private string? SearchQuery { get; set; }

    private bool IsSearching => !string.IsNullOrWhiteSpace(SearchQuery);

    protected override async Task OnInitializedAsync()
    {
        OrdersHubService.OrdersChanged += OnOrdersChanged;

        _ = RefreshTimesAsync();

        await GetOrdersAsync();
    }

    public void Dispose()
    {
        OrdersHubService.OrdersChanged -= OnOrdersChanged;
        _timeRefreshTimer.Dispose();
    }

    private void OnOrdersChanged()
    {
        _ = InvokeAsync(() => GetOrdersAsync(isLiveUpdate: true));
    }

    private async Task OnSearchChangedAsync()
    {
        await GetOrdersAsync();
    }

    // A live update shows no skeleton, and a failed one keeps what is shown
    private async Task GetOrdersAsync(bool isLiveUpdate = false)
    {
        if (!isLiveUpdate)
        {
            IsLoading = true;
        }

        PaginationDto<OrderDto>? pagination = await ApiService.ExecuteAsync(() => OrderApiClient.GetOrdersAsync(
            pageNumber: 1,
            PageSize,
            nameof(OrderSortBy.CreatedAtUtcAsc),
            OrderPaymentFilter.ToPay,
            SearchQuery));

        if (pagination is not null || !isLiveUpdate)
        {
            Orders = pagination?.Items ?? [];
        }

        IsLoading = false;

        StateHasChanged();
    }

    private async Task PayInCashAsync(OrderDto order)
    {
        decimal? cashReceived = await PaymentDialogService.AskForCashAsync(order.Total);

        if (cashReceived is null)
        {
            return;
        }

        await PayAsync(order, new PayOrderRequest(PaymentMethod.Cash, cashReceived));
    }

    private async Task PayByCardAsync(OrderDto order)
    {
        bool isApproved = await PaymentDialogService.ConfirmCardAsync(order.Total);

        if (!isApproved)
        {
            return;
        }

        await PayAsync(order, new PayOrderRequest(PaymentMethod.Card, CashReceived: null));
    }

    // Refused when it was declined meanwhile; the reload then takes it off the list
    private async Task PayAsync(OrderDto order, PayOrderRequest payment)
    {
        PayingOrderId = order.Id;

        OrderDto? paidOrder = await ApiService.ExecuteAsync(async () =>
        {
            await OrderApiClient.PayOrderAsync(order.Id, payment);

            return await OrderApiClient.GetOrderAsync(order.Id);
        });

        PayingOrderId = null;

        await GetOrdersAsync(isLiveUpdate: true);

        if (paidOrder is not null)
        {
            await PaymentDialogService.ShowReceiptAsync(paidOrder);
        }
    }

    private async Task RefreshTimesAsync()
    {
        while (await _timeRefreshTimer.WaitForNextTickAsync())
        {
            await InvokeAsync(StateHasChanged);
        }
    }
}
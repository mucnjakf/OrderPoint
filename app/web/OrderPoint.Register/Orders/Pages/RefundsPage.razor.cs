using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Register.Orders.Api;
using OrderPoint.Register.Orders.Dialogs;
using OrderPoint.Register.Orders.Sorting;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Services;

namespace OrderPoint.Register.Orders.Pages;

// What the register took for orders that were never served, live, oldest first
public sealed partial class RefundsPage : IDisposable
{
    private const int PageSize = 100;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private OrderApiClient OrderApiClient { get; set; } = null!;

    [Inject]
    private OrdersHubService OrdersHubService { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    private IReadOnlyList<OrderDto> Orders { get; set; } = [];

    private bool IsLoading { get; set; } = true;

    private Guid? RefundingOrderId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        OrdersHubService.OrdersChanged += OnOrdersChanged;

        await GetOrdersAsync();
    }

    public void Dispose()
    {
        OrdersHubService.OrdersChanged -= OnOrdersChanged;
    }

    private void OnOrdersChanged()
    {
        _ = InvokeAsync(() => GetOrdersAsync(isLiveUpdate: true));
    }

    // A live update shows no skeleton, and a failed one keeps what is shown
    private async Task GetOrdersAsync(bool isLiveUpdate = false)
    {
        PaginationDto<OrderDto>? pagination = await ApiService.ExecuteAsync(() => OrderApiClient.GetOrdersAsync(
            pageNumber: 1,
            PageSize,
            nameof(OrderSortBy.CreatedAtUtcAsc),
            PaymentStatus.RefundDue));

        if (pagination is not null || !isLiveUpdate)
        {
            Orders = pagination?.Items ?? [];
        }

        IsLoading = false;

        StateHasChanged();
    }

    private async Task HandBackAsync(OrderDto order)
    {
        string total = order.Total.ToString("C", CultureInfo.CurrentCulture);

        var parameters = new DialogParameters<HandBackRefundDialog>
        {
            { dialog => dialog.OrderNumber, order.Number },
            {
                dialog => dialog.Text,
                order.PaymentMethod == PaymentMethod.Cash
                    ? $"Hand {total} back to the guest in cash, then confirm."
                    : $"Refund {total} to the guest's card on the terminal, then confirm."
            }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.ExtraSmall,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<HandBackRefundDialog>(string.Empty, parameters, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        RefundingOrderId = order.Id;

        await ApiService.ExecuteAsync(
            () => OrderApiClient.CompleteRefundAsync(order.Id),
            $"Order #{order.Number} refunded");

        RefundingOrderId = null;

        await GetOrdersAsync(isLiveUpdate: true);
    }
}
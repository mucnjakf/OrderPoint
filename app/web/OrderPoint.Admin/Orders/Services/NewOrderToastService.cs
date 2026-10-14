using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Orders.Api;
using OrderPoint.Admin.Orders.Dialogs;
using OrderPoint.Admin.Orders.Sorting;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Orders.Services;

// One per circuit; MainLayout loads the latest order number after sign-in and checks on every OrdersChanged
internal sealed class NewOrderToastService(
    ApiService apiService,
    OrderApiClient orderApiClient,
    ISnackbar snackbar,
    IDialogService dialogService,
    NavigationManager navigationManager)
{
    private const int CheckedOrdersCount = 5;

    private const string OrdersPageUri = "/orders";

    // Longer than the other snackbars, so there is time to click View
    private const int ToastVisibleMilliseconds = 5000;

    private int? _latestOrderNumber;

    internal async Task LoadAsync()
    {
        IReadOnlyList<OrderDto>? orders = await GetLatestOrdersAsync();

        // Unknown while the API can't be reached, so the next check sets it instead of showing every order as new
        _latestOrderNumber = orders is null ? null : orders.FirstOrDefault()?.Number ?? 0;
    }

    internal void Clear()
    {
        _latestOrderNumber = null;
    }

    internal async Task CheckAsync()
    {
        if (_latestOrderNumber is null)
        {
            await LoadAsync();

            return;
        }

        IReadOnlyList<OrderDto>? orders = await GetLatestOrdersAsync();

        // Checks that overlapped while loading compare against the number the first one stored
        List<OrderDto> newOrders = orders?
            .Where(order => order.Number > _latestOrderNumber)
            .ToList() ?? [];

        if (newOrders.Count == 0)
        {
            return;
        }

        _latestOrderNumber = newOrders.Max(order => order.Number);

        if (newOrders.Count == 1)
        {
            ShowNewOrderToast(newOrders[0]);

            return;
        }

        ShowNewOrdersToast(newOrders.Count);
    }

    private async Task<IReadOnlyList<OrderDto>?> GetLatestOrdersAsync()
    {
        PaginationDto<OrderDto>? pagination = await apiService.ExecuteAsync(
            () => orderApiClient.GetOrdersAsync(
                pageNumber: 1,
                CheckedOrdersCount,
                nameof(OrderSortBy.CreatedAtUtcDesc)));

        return pagination?.Items;
    }

    private void ShowNewOrderToast(OrderDto order)
    {
        string total = order.Total.ToString("C", CultureInfo.CurrentCulture);

        snackbar.Add(
            $"New order #{order.Number} · Table {order.TableCode} · {total}",
            Severity.Info,
            options =>
            {
                options.Action = "View";
                options.ActionVariant = Variant.Text;
                options.RequireInteraction = false;
                options.VisibleStateDuration = ToastVisibleMilliseconds;
                options.OnClick = _ => ShowOrderDetailsDialogAsync(order);
            });
    }

    private void ShowNewOrdersToast(int count)
    {
        snackbar.Add(
            $"{count} new orders",
            Severity.Info,
            options =>
            {
                options.Action = "View";
                options.ActionVariant = Variant.Text;
                options.RequireInteraction = false;
                options.VisibleStateDuration = ToastVisibleMilliseconds;
                options.OnClick = _ => ShowOrdersPageAsync();
            });
    }

    private async Task ShowOrderDetailsDialogAsync(OrderDto order)
    {
        var parameters = new DialogParameters<OrderDetailsDialog>
        {
            { dialog => dialog.Order, order }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        await dialogService
            .ShowAsync<OrderDetailsDialog>(string.Empty, parameters, options);
    }

    private Task ShowOrdersPageAsync()
    {
        navigationManager.NavigateTo(OrdersPageUri);

        return Task.CompletedTask;
    }
}
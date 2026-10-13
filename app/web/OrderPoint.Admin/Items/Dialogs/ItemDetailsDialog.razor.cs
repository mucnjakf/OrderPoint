using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Items.Api;
using OrderPoint.Admin.Items.Dtos;
using OrderPoint.Admin.Orders.Api;
using OrderPoint.Admin.Orders.Sorting;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Items.Dialogs;

public sealed partial class ItemDetailsDialog : IDisposable
{
    private const int PageSize = 5;

    [Parameter]
    public ItemDto Item { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private OrderApiClient OrderApiClient { get; set; } = null!;

    [Inject]
    private ItemApiClient ItemApiClient { get; set; } = null!;

    [Inject]
    private OrdersHubService OrdersHubService { get; set; } = null!;

    private int OrdersCount { get; set; }

    private PaginationDto<OrderDto>? Pagination { get; set; }

    private IReadOnlyList<OrderDto> Orders { get; set; } = [];

    private string UpdatedAtText => Item.UpdatedAtUtc?.ToDisplayDateTime(TimeZoneService.TimeZone) ?? "-";

    private string SelectedSortBy { get; set; } = nameof(OrderSortBy.CreatedAtUtcDesc);

    private string? SearchQuery { get; set; }

    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        OrdersCount = Item.OrdersCount;

        OrdersHubService.OrdersChanged += OnOrdersChanged;

        await GetOrdersAsync(pageNumber: 1);
    }

    public void Dispose()
    {
        OrdersHubService.OrdersChanged -= OnOrdersChanged;
    }

    private void OnOrdersChanged()
    {
        _ = InvokeAsync(() => Task.WhenAll(
            GetOrdersCountAsync(),
            GetOrdersAsync(Pagination?.PageNumber ?? 1, isLiveUpdate: true)));
    }

    private async Task GetOrdersCountAsync()
    {
        ItemDto? item = await ApiService.ExecuteAsync(() => ItemApiClient.GetItemAsync(Item.Id));

        OrdersCount = item?.OrdersCount ?? OrdersCount;

        StateHasChanged();
    }

    private async Task GetOrdersAsync(int pageNumber, bool isLiveUpdate = false)
    {
        if (!isLiveUpdate)
        {
            IsLoading = true;
        }

        Pagination = await ApiService.ExecuteAsync(() => OrderApiClient.GetOrdersAsync(
            pageNumber,
            PageSize,
            SelectedSortBy,
            SearchQuery,
            itemId: Item.Id));

        Orders = Pagination?.Items ?? [];

        IsLoading = false;

        StateHasChanged();
    }

    private async Task OnSearchChangedAsync()
    {
        await GetOrdersAsync(pageNumber: 1);
    }

    private async Task OnSortChangedAsync()
    {
        await GetOrdersAsync(pageNumber: 1);
    }

    private async Task OnPageChangedAsync(int pageNumber)
    {
        await GetOrdersAsync(pageNumber);
    }

    private OrderItemDto GetOrderItem(OrderDto order)
    {
        return order.Items.Single(orderItem => orderItem.ItemId == Item.Id);
    }

    private void Close()
    {
        MudDialogInstance.Close(DialogResult.Ok(true));
    }
}
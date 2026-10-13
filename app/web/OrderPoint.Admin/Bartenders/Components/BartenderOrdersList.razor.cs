using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Orders.Api;
using OrderPoint.Admin.Orders.Sorting;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Bartenders.Components;

public sealed partial class BartenderOrdersList : IDisposable
{
    private const int PageSize = 5;

    [Parameter]
    [EditorRequired]
    public Guid BartenderId { get; set; }

    [Parameter]
    public bool ShowControls { get; set; } = true;

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private OrderApiClient OrderApiClient { get; set; } = null!;

    [Inject]
    private OrdersHubService OrdersHubService { get; set; } = null!;

    private PaginationDto<OrderDto>? Pagination { get; set; }

    private IReadOnlyList<OrderDto> Orders { get; set; } = [];

    private string SelectedSortBy { get; set; } = nameof(OrderSortBy.CreatedAtUtcDesc);

    private string? SearchQuery { get; set; }

    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        OrdersHubService.OrdersChanged += OnOrdersChanged;

        await GetOrdersAsync(pageNumber: 1);
    }

    public void Dispose()
    {
        OrdersHubService.OrdersChanged -= OnOrdersChanged;
    }

    private void OnOrdersChanged()
    {
        _ = InvokeAsync(() => GetOrdersAsync(Pagination?.PageNumber ?? 1, isLiveUpdate: true));
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
            bartenderId: BartenderId));

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

    private static string GetItemsCountText(OrderDto order)
    {
        int itemsCount = order.Items.Sum(orderItem => orderItem.Quantity);

        return itemsCount.ToCountText("item", "items");
    }
}
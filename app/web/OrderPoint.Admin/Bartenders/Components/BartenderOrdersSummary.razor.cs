using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Orders.Api;
using OrderPoint.Admin.Orders.Sorting;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Bartenders.Components;

// Rendered only while the bartender's row is expanded, so the last order loads on expand
public sealed partial class BartenderOrdersSummary : IDisposable
{
    [Parameter]
    [EditorRequired]
    public Guid BartenderId { get; set; }

    [Parameter]
    [EditorRequired]
    public int OrdersCount { get; set; }

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private OrderApiClient OrderApiClient { get; set; } = null!;

    [Inject]
    private OrdersHubService OrdersHubService { get; set; } = null!;

    private OrderDto? LastOrder { get; set; }

    protected override async Task OnInitializedAsync()
    {
        OrdersHubService.OrdersChanged += OnOrdersChanged;

        await GetLastOrderAsync();
    }

    public void Dispose()
    {
        OrdersHubService.OrdersChanged -= OnOrdersChanged;
    }

    private void OnOrdersChanged()
    {
        _ = InvokeAsync(GetLastOrderAsync);
    }

    private async Task GetLastOrderAsync()
    {
        PaginationDto<OrderDto>? pagination = await ApiService.ExecuteAsync(
            () => OrderApiClient.GetOrdersAsync(
                pageNumber: 1,
                pageSize: 1,
                nameof(OrderSortBy.CreatedAtUtcDesc),
                bartenderId: BartenderId));

        LastOrder = pagination?.Items.FirstOrDefault();

        StateHasChanged();
    }
}
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Orders.Api;
using OrderPoint.Admin.Orders.Dialogs;
using OrderPoint.Admin.Orders.Dtos;
using OrderPoint.Admin.Orders.Enumerations;
using OrderPoint.Admin.Orders.Sorting;
using OrderPoint.Admin.Shared.Dtos;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Orders.Pages;

public sealed partial class OrdersPage
{
    private const int PageSize = 10;

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private OrderApiClient OrderApiClient { get; set; } = null!;

    private List<BreadcrumbItem> Breadcrumbs { get; set; } =
    [
        new("Dashboard", href: "/", icon: Icons.Material.Filled.Dashboard),
        new("Orders", href: null, disabled: true, icon: Icons.Material.Filled.Receipt)
    ];

    private Dictionary<OrderStatus, int> StatusCounts { get; } = [];

    private HashSet<OrderStatus> LoadingStatuses { get; } = [.. Enum.GetValues<OrderStatus>()];

    private PaginationDto<OrderDto>? Pagination { get; set; }

    private IReadOnlyList<OrderDto> Orders { get; set; } = [];

    private string SelectedSortBy { get; set; } = nameof(OrderSortBy.CreatedAtUtcDesc);

    private string? SearchQuery { get; set; }

    private OrderStatus? SelectedStatus { get; set; }

    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(
            GetStatusCountsAsync(),
            GetOrdersAsync(pageNumber: 1));
    }

    private async Task GetStatusCountsAsync()
    {
        await Task.WhenAll(Enum.GetValues<OrderStatus>().Select(GetStatusCountAsync));
    }

    private async Task GetStatusCountAsync(OrderStatus status)
    {
        LoadingStatuses.Add(status);

        PaginationDto<OrderDto>? pagination = await ApiService.ExecuteAsync(
            () => OrderApiClient.GetOrdersAsync(
                pageNumber: 1,
                pageSize: 1,
                nameof(OrderSortBy.CreatedAtUtcDesc),
                status: status));

        StatusCounts[status] = pagination?.TotalCount ?? 0;

        LoadingStatuses.Remove(status);

        StateHasChanged();
    }

    private async Task GetOrdersAsync(int pageNumber)
    {
        IsLoading = true;

        Pagination = await ApiService.ExecuteAsync(() => OrderApiClient.GetOrdersAsync(
            pageNumber,
            PageSize,
            SelectedSortBy,
            SearchQuery,
            SelectedStatus));

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

    private async Task OnStatusClickAsync(OrderStatus status)
    {
        SelectedStatus = SelectedStatus == status ? null : status;

        await GetOrdersAsync(pageNumber: 1);
    }

    private async Task OnPageChangedAsync(int pageNumber)
    {
        await GetOrdersAsync(pageNumber);
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

        await DialogService
            .ShowAsync<OrderDetailsDialog>(string.Empty, parameters, options);
    }
}
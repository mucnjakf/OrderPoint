using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Bartenders.Dtos;
using OrderPoint.Admin.Orders.Api;
using OrderPoint.Admin.Orders.Dtos;
using OrderPoint.Admin.Orders.Sorting;
using OrderPoint.Admin.Shared.Dtos;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Bartenders.Dialogs;

public sealed partial class BartenderOrdersDialog
{
    private const int PageSize = 5;

    [Parameter]
    public BartenderDto Bartender { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private OrderApiClient OrderApiClient { get; set; } = null!;

    private PaginationDto<OrderDto>? Pagination { get; set; }

    private IReadOnlyList<OrderDto> Orders { get; set; } = [];

    private string SelectedSortBy { get; set; } = nameof(OrderSortBy.CreatedAtUtcDesc);

    private string? SearchQuery { get; set; }

    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        await GetOrdersAsync(pageNumber: 1);
    }

    private async Task GetOrdersAsync(int pageNumber)
    {
        IsLoading = true;

        Pagination = await ApiService.ExecuteAsync(() => OrderApiClient.GetOrdersAsync(
            pageNumber,
            PageSize,
            SelectedSortBy,
            SearchQuery,
            bartenderId: Bartender.Id));

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

        return itemsCount == 1 ? "1 item" : $"{itemsCount} items";
    }

    private void Close()
    {
        MudDialogInstance.Close(DialogResult.Ok(true));
    }
}
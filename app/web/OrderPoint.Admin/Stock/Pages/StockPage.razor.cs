using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Stock.Api;
using OrderPoint.Admin.Stock.Api.Requests;
using OrderPoint.Admin.Stock.Dialogs;
using OrderPoint.Admin.Stock.Dtos;
using OrderPoint.Admin.Stock.Enumerations;
using OrderPoint.Admin.Stock.Services;
using OrderPoint.Admin.Stock.Sorting;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Stock.Pages;

// The stock sheet: stock items under their groups, with the value on the shelves and what is low or out above it
public sealed partial class StockPage : IDisposable
{
    // A sheet worth scanning, not ten rows at a time
    private const int PageSize = 50;

    private const string ColumnTitleStyle = "color: var(--mud-palette-text-secondary)";

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private StockApiClient StockApiClient { get; set; } = null!;

    [Inject]
    private StockDialogService StockDialogService { get; set; } = null!;

    [Inject]
    private OrdersHubService OrdersHubService { get; set; } = null!;

    private StockSummaryDto? Summary { get; set; }

    private PaginationDto<StockItemDto>? Pagination { get; set; }

    private IReadOnlyList<StockItemDto> StockItems { get; set; } = [];

    private string SelectedSortBy { get; set; } = nameof(StockItemSortBy.NameAsc);

    private string? SearchQuery { get; set; }

    private StockGroup? SelectedGroup { get; set; }

    private StockLevel? SelectedLevel { get; set; }

    private StockItemStatus? SelectedStatus { get; set; }

    private bool IsLoadingSummary { get; set; } = true;

    private int ReorderCount => (Summary?.LowCount ?? 0) + (Summary?.OutCount ?? 0);

    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        OrdersHubService.OrdersChanged += OnOrdersChanged;

        await Task.WhenAll(
            GetSummaryAsync(),
            GetStockItemsAsync(pageNumber: 1));
    }

    public void Dispose()
    {
        OrdersHubService.OrdersChanged -= OnOrdersChanged;
    }

    // A completed order takes its ingredients off the stock
    private void OnOrdersChanged()
    {
        _ = InvokeAsync(() => Task.WhenAll(
            GetSummaryAsync(),
            GetStockItemsAsync(Pagination?.PageNumber ?? 1, isLiveUpdate: true)));
    }

    private async Task GetSummaryAsync()
    {
        Summary = await ApiService.ExecuteAsync(() => StockApiClient.GetStockSummaryAsync());

        IsLoadingSummary = false;

        StateHasChanged();
    }

    private async Task GetStockItemsAsync(int pageNumber, bool isLiveUpdate = false)
    {
        if (!isLiveUpdate)
        {
            IsLoading = true;
        }

        Pagination = await ApiService.ExecuteAsync(() => StockApiClient.GetStockItemsAsync(
            pageNumber,
            PageSize,
            SelectedSortBy,
            SearchQuery,
            SelectedGroup,
            SelectedLevel,
            SelectedStatus));

        StockItems = Pagination?.Items ?? [];

        IsLoading = false;

        StateHasChanged();
    }

    // Both change with every stock change
    private async Task ReloadAsync()
    {
        await Task.WhenAll(
            GetSummaryAsync(),
            GetStockItemsAsync(Pagination?.PageNumber ?? 1));
    }

    private async Task OnFiltersChangedAsync()
    {
        await GetStockItemsAsync(pageNumber: 1);
    }

    private async Task OnPageChangedAsync(int pageNumber)
    {
        await GetStockItemsAsync(pageNumber);
    }

    // Like the Orders pipeline: a second click clears the filter
    private async Task OnLevelCardClickAsync(StockLevel level)
    {
        SelectedLevel = SelectedLevel == level ? null : level;

        await GetStockItemsAsync(pageNumber: 1);
    }

    private static string GetGroupValueText(IReadOnlyList<StockItemDto> stockItems)
    {
        return stockItems.Sum(stockItem => stockItem.Value).ToString("C", CultureInfo.CurrentCulture);
    }

    private async Task RecordWasteAsync(StockItemDto stockItem)
    {
        if (await StockDialogService.RecordWasteAsync(stockItem))
        {
            await ReloadAsync();
        }
    }

    private async Task CorrectAsync(StockItemDto stockItem)
    {
        if (await StockDialogService.CorrectAsync(stockItem))
        {
            await ReloadAsync();
        }
    }

    private async Task ShowStockItemDetailsDialogAsync(StockItemDto stockItem)
    {
        var parameters = new DialogParameters<StockItemDetailsDialog>
        {
            { dialog => dialog.StockItem, stockItem },
            { dialog => dialog.OnStockChanged, EventCallback.Factory.Create(this, ReloadAsync) }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        await DialogService.ShowAsync<StockItemDetailsDialog>(string.Empty, parameters, options);
    }

    private async Task ShowReorderDialogAsync()
    {
        var parameters = new DialogParameters<ReorderDialog>
        {
            { dialog => dialog.OnStockChanged, EventCallback.Factory.Create(this, ReloadAsync) }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        await DialogService.ShowAsync<ReorderDialog>(string.Empty, parameters, options);
    }

    private async Task ShowCreateStockItemDialogAsync()
    {
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<CreateStockItemDialog>(string.Empty, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        var request = (dialogResult.Data as CreateStockItemRequest)!;

        bool isSuccess = await ApiService.ExecuteAsync(
            () => StockApiClient.CreateStockItemAsync(request),
            $"Stock item {request.Name} created successfully");

        if (isSuccess)
        {
            await ReloadAsync();
        }
    }

    private async Task ShowUpdateStockItemDialogAsync(StockItemDto stockItem)
    {
        var parameters = new DialogParameters<UpdateStockItemDialog>
        {
            { dialog => dialog.StockItem, stockItem }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<UpdateStockItemDialog>(string.Empty, parameters, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        var request = (dialogResult.Data as UpdateStockItemRequest)!;

        bool isSuccess = await ApiService.ExecuteAsync(
            () => StockApiClient.UpdateStockItemAsync(stockItem.Id, request),
            $"Stock item {request.Name} edited successfully");

        if (isSuccess)
        {
            await ReloadAsync();
        }
    }

    private async Task ShowDeleteStockItemDialogAsync(StockItemDto stockItem)
    {
        var parameters = new DialogParameters<DeleteStockItemDialog>
        {
            { dialog => dialog.StockItem, stockItem }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<DeleteStockItemDialog>(string.Empty, parameters, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        bool isSuccess = await ApiService.ExecuteAsync(
            () => StockApiClient.DeleteStockItemAsync(stockItem.Id),
            $"Stock item {stockItem.Name} deleted successfully");

        if (isSuccess)
        {
            await ReloadAsync();
        }
    }
}
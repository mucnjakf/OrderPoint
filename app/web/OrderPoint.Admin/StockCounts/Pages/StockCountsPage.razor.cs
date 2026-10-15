using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.StockCounts.Api;
using OrderPoint.Admin.StockCounts.Api.Requests;
using OrderPoint.Admin.StockCounts.Dialogs;
using OrderPoint.Admin.StockCounts.Dtos;
using OrderPoint.Admin.StockCounts.Enumerations;
using OrderPoint.Admin.StockCounts.Sorting;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.StockCounts.Pages;

// The count in progress, if any, and the closed counts with what they found
public sealed partial class StockCountsPage
{
    private const int PageSize = 10;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private StockCountApiClient StockCountApiClient { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private StockCountDto? OpenCount { get; set; }

    private PaginationDto<StockCountDto>? Pagination { get; set; }

    private IReadOnlyList<StockCountDto> StockCounts { get; set; } = [];

    private string SelectedSortBy { get; set; } = nameof(StockCountSortBy.ClosedAtUtcDesc);

    private string? SearchQuery { get; set; }

    private bool IsLoadingOpenCount { get; set; } = true;

    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(
            GetOpenCountAsync(),
            GetStockCountsAsync(pageNumber: 1));
    }

    // At most one count is open
    private async Task GetOpenCountAsync()
    {
        PaginationDto<StockCountDto>? openCounts = await ApiService.ExecuteAsync(
            () => StockCountApiClient.GetStockCountsAsync(
                pageNumber: 1,
                pageSize: 1,
                nameof(StockCountSortBy.ClosedAtUtcDesc),
                status: StockCountStatus.Open));

        OpenCount = openCounts?.Items.FirstOrDefault();

        IsLoadingOpenCount = false;

        StateHasChanged();
    }

    private async Task GetStockCountsAsync(int pageNumber)
    {
        IsLoading = true;

        Pagination = await ApiService.ExecuteAsync(() => StockCountApiClient.GetStockCountsAsync(
            pageNumber,
            PageSize,
            SelectedSortBy,
            SearchQuery,
            StockCountStatus.Closed));

        StockCounts = Pagination?.Items ?? [];

        IsLoading = false;

        StateHasChanged();
    }

    private async Task OnFiltersChangedAsync()
    {
        await GetStockCountsAsync(pageNumber: 1);
    }

    private async Task OnPageChangedAsync(int pageNumber)
    {
        await GetStockCountsAsync(pageNumber);
    }

    private async Task ShowStartStockCountDialogAsync()
    {
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<StartStockCountDialog>(string.Empty, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        var request = (dialogResult.Data as StartStockCountRequest)!;

        StockCountDto? stockCount = await ApiService.ExecuteAsync(
            () => StockCountApiClient.StartStockCountAsync(request));

        if (stockCount is not null)
        {
            NavigationManager.NavigateTo($"/counts/{stockCount.Id}");
        }
    }

    private async Task ShowDiscardStockCountDialogAsync()
    {
        if (OpenCount is null)
        {
            return;
        }

        var parameters = new DialogParameters<DiscardStockCountDialog>
        {
            { dialog => dialog.StockCount, OpenCount }
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<DiscardStockCountDialog>(string.Empty, parameters);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        Guid id = OpenCount.Id;

        bool isSuccess = await ApiService.ExecuteAsync(
            () => StockCountApiClient.DiscardStockCountAsync(id),
            "Count discarded");

        if (isSuccess)
        {
            await GetOpenCountAsync();
        }
    }

    private async Task ShowStockCountDetailsDialogAsync(StockCountDto stockCount)
    {
        var parameters = new DialogParameters<StockCountDetailsDialog>
        {
            { dialog => dialog.StockCount, stockCount }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        await DialogService.ShowAsync<StockCountDetailsDialog>(string.Empty, parameters, options);
    }
}
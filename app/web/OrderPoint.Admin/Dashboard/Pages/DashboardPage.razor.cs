using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Bartenders.Api;
using OrderPoint.Admin.Bartenders.Dialogs;
using OrderPoint.Admin.Bartenders.Dtos;
using OrderPoint.Admin.Dashboard.Api;
using OrderPoint.Admin.Dashboard.Dtos;
using OrderPoint.Admin.Dashboard.Enumerations;
using OrderPoint.Admin.Items.Api;
using OrderPoint.Admin.Items.Dialogs;
using OrderPoint.Admin.Items.Dtos;
using OrderPoint.Admin.Orders.Api;
using OrderPoint.Admin.Orders.Dialogs;
using OrderPoint.Admin.Orders.Sorting;
using OrderPoint.Admin.Stock.Api;
using OrderPoint.Admin.Stock.Dialogs;
using OrderPoint.Admin.Stock.Dtos;
using OrderPoint.Admin.Stock.Enumerations;
using OrderPoint.Admin.Stock.Sorting;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Dashboard.Pages;

public sealed partial class DashboardPage : IDisposable
{
    private const int RecentOrdersCount = 5;

    private const int LowStockCount = 5;

    private const int LiveRefreshIntervalInSeconds = 30;

    private readonly PeriodicTimer _liveRefreshTimer = new(TimeSpan.FromSeconds(LiveRefreshIntervalInSeconds));

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private DashboardApiClient DashboardApiClient { get; set; } = null!;

    [Inject]
    private OrderApiClient OrderApiClient { get; set; } = null!;

    [Inject]
    private ItemApiClient ItemApiClient { get; set; } = null!;

    [Inject]
    private BartenderApiClient BartenderApiClient { get; set; } = null!;

    [Inject]
    private StockApiClient StockApiClient { get; set; } = null!;

    [Inject]
    private OrdersHubService OrdersHubService { get; set; } = null!;

    private NewOrdersTracker NewRecentOrders { get; } = new();

    private DashboardPeriod SelectedPeriod { get; set; } = DashboardPeriod.Last7Days;

    private DashboardSummaryDto? Summary { get; set; }

    private DashboardLiveDto? Live { get; set; }

    private DateTimeOffset LiveUpdatedAtUtc { get; set; }

    private IReadOnlyList<RevenueTrendPointDto> RevenueTrend { get; set; } = [];

    private IReadOnlyList<BusiestTimeDto> BusiestTimes { get; set; } = [];

    private IReadOnlyList<CategoryRevenueDto> CategoryRevenue { get; set; } = [];

    private PaymentMethodsDto? PaymentMethods { get; set; }

    private GrossProfitDto? GrossProfit { get; set; }

    private StockSummaryDto? StockSummary { get; set; }

    private IReadOnlyList<StockItemDto> LowStockItems { get; set; } = [];

    private IReadOnlyList<TopItemDto> TopItems { get; set; } = [];

    private IReadOnlyList<BartenderLeaderboardEntryDto> Leaderboard { get; set; } = [];

    private IReadOnlyList<OrderDto> RecentOrders { get; set; } = [];

    private bool IsLoadingSummary { get; set; } = true;

    private bool IsLoadingLive { get; set; } = true;

    private bool IsLoadingRevenueTrend { get; set; } = true;

    private bool IsLoadingBusiestTimes { get; set; } = true;

    private bool IsLoadingCategoryRevenue { get; set; } = true;

    private bool IsLoadingPaymentMethods { get; set; } = true;

    private bool HasPayments =>
        PaymentMethods is { } methods && methods.CashCount + methods.CardCount + methods.OnlineCardCount > 0;

    private bool IsLoadingGrossProfit { get; set; } = true;

    private bool HasGrossProfit => GrossProfit is { } grossProfit &&
                                   (grossProfit.Revenue != 0 ||
                                    grossProfit.CostOfGoods != 0 ||
                                    grossProfit.WasteValue != 0 ||
                                    grossProfit.CountDifferenceValue != 0);

    private bool IsLoadingLowStock { get; set; } = true;

    private bool IsLoadingTopItems { get; set; } = true;

    private bool IsLoadingLeaderboard { get; set; } = true;

    private bool IsLoadingRecentOrders { get; set; } = true;

    private string ServiceTimeText => Summary?.AverageServiceMinutes is null
        ? "-"
        : $"{Summary.AverageServiceMinutes.Value:0} min";

    protected override async Task OnInitializedAsync()
    {
        OrdersHubService.OrdersChanged += OnOrdersChanged;

        _ = RefreshLiveAsync();

        await GetSectionsAsync();
    }

    public void Dispose()
    {
        OrdersHubService.OrdersChanged -= OnOrdersChanged;
        _liveRefreshTimer.Dispose();
    }

    // The loading flags start true and only a period change sets them again, so live updates swap data in place
    private void OnOrdersChanged()
    {
        _ = InvokeAsync(GetSectionsAsync);
    }

    private async Task OnPeriodChangedAsync()
    {
        IsLoadingSummary = true;
        IsLoadingRevenueTrend = true;
        IsLoadingCategoryRevenue = true;
        IsLoadingPaymentMethods = true;
        IsLoadingGrossProfit = true;
        IsLoadingTopItems = true;
        IsLoadingLeaderboard = true;

        await GetPeriodSectionsAsync();
    }

    private async Task GetSectionsAsync()
    {
        await Task.WhenAll(
            GetPeriodSectionsAsync(),
            GetLiveAsync(),
            GetBusiestTimesAsync(),
            GetLowStockAsync(),
            GetRecentOrdersAsync());
    }

    private async Task GetPeriodSectionsAsync()
    {
        await Task.WhenAll(
            GetSummaryAsync(),
            GetRevenueTrendAsync(),
            GetCategoryRevenueAsync(),
            GetPaymentMethodsAsync(),
            GetGrossProfitAsync(),
            GetTopItemsAsync(),
            GetLeaderboardAsync());
    }

    private async Task RefreshLiveAsync()
    {
        while (await _liveRefreshTimer.WaitForNextTickAsync())
        {
            await InvokeAsync(GetLiveAsync);
        }
    }

    private async Task GetSummaryAsync()
    {
        Summary = await ApiService.ExecuteAsync(() => DashboardApiClient.GetSummaryAsync(SelectedPeriod));

        IsLoadingSummary = false;

        StateHasChanged();
    }

    private async Task GetLiveAsync()
    {
        Live = await ApiService.ExecuteAsync(() => DashboardApiClient.GetLiveAsync());
        LiveUpdatedAtUtc = DateTimeOffset.UtcNow;

        IsLoadingLive = false;

        StateHasChanged();
    }

    private async Task GetRevenueTrendAsync()
    {
        IReadOnlyList<RevenueTrendPointDto>? revenueTrend = await ApiService.ExecuteAsync(
            () => DashboardApiClient.GetRevenueTrendAsync(SelectedPeriod));

        RevenueTrend = revenueTrend ?? [];

        IsLoadingRevenueTrend = false;

        StateHasChanged();
    }

    private async Task GetBusiestTimesAsync()
    {
        IReadOnlyList<BusiestTimeDto>? busiestTimes = await ApiService.ExecuteAsync(
            () => DashboardApiClient.GetBusiestTimesAsync());

        BusiestTimes = busiestTimes ?? [];

        IsLoadingBusiestTimes = false;

        StateHasChanged();
    }

    private async Task GetPaymentMethodsAsync()
    {
        PaymentMethods = await ApiService.ExecuteAsync(
            () => DashboardApiClient.GetPaymentMethodsAsync(SelectedPeriod));

        IsLoadingPaymentMethods = false;

        StateHasChanged();
    }

    private async Task GetGrossProfitAsync()
    {
        GrossProfit = await ApiService.ExecuteAsync(() => DashboardApiClient.GetGrossProfitAsync(SelectedPeriod));

        IsLoadingGrossProfit = false;

        StateHasChanged();
    }

    // Out first, then low, from the stock sheet's own list
    private async Task GetLowStockAsync()
    {
        Task<StockSummaryDto?> summaryTask = ApiService.ExecuteAsync(() => StockApiClient.GetStockSummaryAsync());
        Task<PaginationDto<StockItemDto>?> outTask = GetStockItemsByLevelAsync(StockLevel.Out);
        Task<PaginationDto<StockItemDto>?> lowTask = GetStockItemsByLevelAsync(StockLevel.Low);

        await Task.WhenAll(summaryTask, outTask, lowTask);

        StockSummary = await summaryTask;
        PaginationDto<StockItemDto>? outStockItems = await outTask;
        PaginationDto<StockItemDto>? lowStockItems = await lowTask;

        LowStockItems = (outStockItems?.Items ?? [])
            .Concat(lowStockItems?.Items ?? [])
            .Take(LowStockCount)
            .ToList();

        IsLoadingLowStock = false;

        StateHasChanged();
    }

    private Task<PaginationDto<StockItemDto>?> GetStockItemsByLevelAsync(StockLevel level)
    {
        return ApiService.ExecuteAsync(() => StockApiClient.GetStockItemsAsync(
            pageNumber: 1,
            LowStockCount,
            nameof(StockItemSortBy.NameAsc),
            level: level,
            status: StockItemStatus.Active));
    }

    private async Task GetCategoryRevenueAsync()
    {
        IReadOnlyList<CategoryRevenueDto>? categoryRevenue = await ApiService.ExecuteAsync(
            () => DashboardApiClient.GetCategoryRevenueAsync(SelectedPeriod));

        CategoryRevenue = categoryRevenue ?? [];

        IsLoadingCategoryRevenue = false;

        StateHasChanged();
    }

    private async Task GetTopItemsAsync()
    {
        IReadOnlyList<TopItemDto>? topItems = await ApiService.ExecuteAsync(
            () => DashboardApiClient.GetTopItemsAsync(SelectedPeriod));

        TopItems = topItems ?? [];

        IsLoadingTopItems = false;

        StateHasChanged();
    }

    private async Task GetLeaderboardAsync()
    {
        IReadOnlyList<BartenderLeaderboardEntryDto>? leaderboard = await ApiService.ExecuteAsync(
            () => DashboardApiClient.GetBartenderLeaderboardAsync(SelectedPeriod));

        Leaderboard = leaderboard ?? [];

        IsLoadingLeaderboard = false;

        StateHasChanged();
    }

    private async Task GetRecentOrdersAsync()
    {
        DateTimeOffset loadStartedAtUtc = DateTimeOffset.UtcNow;

        PaginationDto<OrderDto>? pagination = await ApiService.ExecuteAsync(
            () => OrderApiClient.GetOrdersAsync(
                pageNumber: 1,
                RecentOrdersCount,
                nameof(OrderSortBy.CreatedAtUtcDesc)));

        RecentOrders = pagination?.Items ?? [];

        NewRecentOrders.Track(RecentOrders, loadStartedAtUtc, () => InvokeAsync(StateHasChanged));

        IsLoadingRecentOrders = false;

        StateHasChanged();
    }

    private async Task ShowItemDetailsDialogAsync(TopItemDto topItem)
    {
        ItemDto? item = await ApiService.ExecuteAsync(() => ItemApiClient.GetItemAsync(topItem.ItemId));

        if (item is null)
        {
            return;
        }

        var parameters = new DialogParameters<ItemDetailsDialog>
        {
            { dialog => dialog.Item, item }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        await DialogService
            .ShowAsync<ItemDetailsDialog>(string.Empty, parameters, options);
    }

    private async Task ShowBartenderOrdersDialogAsync(BartenderLeaderboardEntryDto entry)
    {
        BartenderDto? bartender = await ApiService.ExecuteAsync(
            () => BartenderApiClient.GetBartenderAsync(entry.BartenderId));

        if (bartender is null)
        {
            return;
        }

        var parameters = new DialogParameters<BartenderOrdersDialog>
        {
            { dialog => dialog.Bartender, bartender }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        await DialogService
            .ShowAsync<BartenderOrdersDialog>(string.Empty, parameters, options);
    }

    private async Task ShowStockItemDetailsDialogAsync(StockItemDto stockItem)
    {
        var parameters = new DialogParameters<StockItemDetailsDialog>
        {
            { dialog => dialog.StockItem, stockItem },
            { dialog => dialog.OnStockChanged, EventCallback.Factory.Create(this, ReloadStockSectionsAsync) }
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
            { dialog => dialog.OnStockChanged, EventCallback.Factory.Create(this, ReloadStockSectionsAsync) }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        await DialogService.ShowAsync<ReorderDialog>(string.Empty, parameters, options);
    }

    // Waste, corrections and deliveries change the stock and the losses
    private async Task ReloadStockSectionsAsync()
    {
        await Task.WhenAll(
            GetLowStockAsync(),
            GetGrossProfitAsync());
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
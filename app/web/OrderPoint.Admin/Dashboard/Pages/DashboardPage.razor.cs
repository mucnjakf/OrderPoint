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
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Dashboard.Pages;

public sealed partial class DashboardPage : IDisposable
{
    private const int RecentOrdersCount = 5;

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
    private OrdersHubService OrdersHubService { get; set; } = null!;

    private NewOrdersTracker NewRecentOrders { get; } = new();

    private DashboardPeriod SelectedPeriod { get; set; } = DashboardPeriod.Last7Days;

    private DashboardSummaryDto? Summary { get; set; }

    private DashboardLiveDto? Live { get; set; }

    private DateTimeOffset LiveUpdatedAtUtc { get; set; }

    private IReadOnlyList<RevenueTrendPointDto> RevenueTrend { get; set; } = [];

    private IReadOnlyList<BusiestTimeDto> BusiestTimes { get; set; } = [];

    private IReadOnlyList<CategoryRevenueDto> CategoryRevenue { get; set; } = [];

    private IReadOnlyList<TopItemDto> TopItems { get; set; } = [];

    private IReadOnlyList<BartenderLeaderboardEntryDto> Leaderboard { get; set; } = [];

    private IReadOnlyList<OrderDto> RecentOrders { get; set; } = [];

    private bool IsLoadingSummary { get; set; } = true;

    private bool IsLoadingLive { get; set; } = true;

    private bool IsLoadingRevenueTrend { get; set; } = true;

    private bool IsLoadingBusiestTimes { get; set; } = true;

    private bool IsLoadingCategoryRevenue { get; set; } = true;

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
            GetRecentOrdersAsync());
    }

    private async Task GetPeriodSectionsAsync()
    {
        await Task.WhenAll(
            GetSummaryAsync(),
            GetRevenueTrendAsync(),
            GetCategoryRevenueAsync(),
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
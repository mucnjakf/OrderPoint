using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Dashboard.Api;
using OrderPoint.Admin.Dashboard.Dtos;
using OrderPoint.Admin.Dashboard.Enumerations;
using OrderPoint.Admin.Orders.Api;
using OrderPoint.Admin.Orders.Dialogs;
using OrderPoint.Admin.Orders.Dtos;
using OrderPoint.Admin.Orders.Sorting;
using OrderPoint.Admin.Shared.Dtos;
using OrderPoint.Admin.Shared.Services;

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
        _ = RefreshLiveAsync();

        await Task.WhenAll(
            GetPeriodSectionsAsync(),
            GetLiveAsync(),
            GetBusiestTimesAsync(),
            GetRecentOrdersAsync());
    }

    public void Dispose()
    {
        _liveRefreshTimer.Dispose();
    }

    private async Task OnPeriodChangedAsync()
    {
        await GetPeriodSectionsAsync();
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
        IsLoadingSummary = true;

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
        IsLoadingRevenueTrend = true;

        IReadOnlyList<RevenueTrendPointDto>? revenueTrend = await ApiService.ExecuteAsync(
            () => DashboardApiClient.GetRevenueTrendAsync(SelectedPeriod));

        RevenueTrend = revenueTrend ?? [];

        IsLoadingRevenueTrend = false;

        StateHasChanged();
    }

    private async Task GetBusiestTimesAsync()
    {
        IsLoadingBusiestTimes = true;

        IReadOnlyList<BusiestTimeDto>? busiestTimes = await ApiService.ExecuteAsync(
            () => DashboardApiClient.GetBusiestTimesAsync());

        BusiestTimes = busiestTimes ?? [];

        IsLoadingBusiestTimes = false;

        StateHasChanged();
    }

    private async Task GetCategoryRevenueAsync()
    {
        IsLoadingCategoryRevenue = true;

        IReadOnlyList<CategoryRevenueDto>? categoryRevenue = await ApiService.ExecuteAsync(
            () => DashboardApiClient.GetCategoryRevenueAsync(SelectedPeriod));

        CategoryRevenue = categoryRevenue ?? [];

        IsLoadingCategoryRevenue = false;

        StateHasChanged();
    }

    private async Task GetTopItemsAsync()
    {
        IsLoadingTopItems = true;

        IReadOnlyList<TopItemDto>? topItems = await ApiService.ExecuteAsync(
            () => DashboardApiClient.GetTopItemsAsync(SelectedPeriod));

        TopItems = topItems ?? [];

        IsLoadingTopItems = false;

        StateHasChanged();
    }

    private async Task GetLeaderboardAsync()
    {
        IsLoadingLeaderboard = true;

        IReadOnlyList<BartenderLeaderboardEntryDto>? leaderboard = await ApiService.ExecuteAsync(
            () => DashboardApiClient.GetBartenderLeaderboardAsync(SelectedPeriod));

        Leaderboard = leaderboard ?? [];

        IsLoadingLeaderboard = false;

        StateHasChanged();
    }

    private async Task GetRecentOrdersAsync()
    {
        IsLoadingRecentOrders = true;

        PaginationDto<OrderDto>? pagination = await ApiService.ExecuteAsync(
            () => OrderApiClient.GetOrdersAsync(
                pageNumber: 1,
                RecentOrdersCount,
                nameof(OrderSortBy.CreatedAtUtcDesc)));

        RecentOrders = pagination?.Items ?? [];

        IsLoadingRecentOrders = false;

        StateHasChanged();
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
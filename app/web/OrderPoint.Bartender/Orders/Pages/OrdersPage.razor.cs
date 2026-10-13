using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Bartender.Orders.Api;
using OrderPoint.Bartender.Orders.Enumerations;
using OrderPoint.Bartender.Orders.Services;
using OrderPoint.Bartender.Orders.Sorting;
using OrderPoint.Bartender.Shared.Extensions;
using OrderPoint.Shared.Auth.Services;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Services;

namespace OrderPoint.Bartender.Orders.Pages;

// Queue: every pending order, oldest first. Mine: my active, then my accepted orders. Done: what I finished today.
// From 600 px (tablets) the selected order shows next to the list; phones open it on its own page.
public sealed partial class OrdersPage : IAsyncDisposable
{
    // Far more than one bartender handles at once or in a shift, so no paging
    private const int MaxOrdersPerStatus = 50;

    // "Placed 3 min ago" moves on even when nothing changes
    private const int TimeRefreshIntervalInSeconds = 30;

    // A row of equal buttons rather than MudTabs, whose tab bar only grows as wide as its tabs; the bar is a paper
    // like the tickets (border, theme radius) and clips the square buttons to its rounded corners
    private const string TabBarStyle = "overflow: hidden;";

    // The tabs stay right under the dense app bar while scrolling; the wrapper's background hides the tickets passing
    // underneath, and its padding replaces the gap to the list
    private const string TabBarWrapperStyle =
        "position: sticky; top: 48px; z-index: 2; padding: 12px 0; margin-top: -12px; " +
        "background-color: var(--mud-palette-background);";

    private const string ActiveTabStyle = "border-radius: 0; border-bottom: 2px solid var(--mud-palette-primary);";

    private const string InactiveTabStyle =
        "border-radius: 0; border-bottom: 2px solid transparent; color: var(--mud-palette-text-secondary);";

    // Below the dense app bar and as tall as the space above the bottom navigation, so the order's buttons sit at
    // the bottom of the screen
    private const string DetailsPanelStyle = "position: sticky; top: 60px; height: calc(100vh - 60px - 76px);";

    private readonly Guid _viewportSubscriptionId = Guid.NewGuid();

    private readonly PeriodicTimer _timeRefreshTimer = new(TimeSpan.FromSeconds(TimeRefreshIntervalInSeconds));

    [Inject]
    private OrdersTabState OrdersTabState { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private OrderApiClient OrderApiClient { get; set; } = null!;

    [Inject]
    private AuthService AuthService { get; set; } = null!;

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Inject]
    private OrdersHubService OrdersHubService { get; set; } = null!;

    [Inject]
    private IBrowserViewportService BrowserViewportService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private NewOrdersTracker NewQueueOrders { get; } = new();

    private bool IsSplitView { get; set; }

    private Guid? SelectedOrderId { get; set; }

    private IReadOnlyList<OrderDto> QueueOrders { get; set; } = [];

    private IReadOnlyList<OrderDto> MyOrders { get; set; } = [];

    private IReadOnlyList<OrderDto> DoneOrders { get; set; } = [];

    private int QueueCount { get; set; }

    private int MineCount { get; set; }

    private bool IsLoadingQueue { get; set; } = true;

    private bool IsLoadingMine { get; set; } = true;

    private bool IsLoadingDone { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        OrdersHubService.OrdersChanged += OnOrdersChanged;

        _ = RefreshTimesAsync();

        await GetTabsAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        await BrowserViewportService.SubscribeAsync(
            _viewportSubscriptionId,
            OnViewportChangedAsync,
            options: null,
            fireImmediately: true);
    }

    public async ValueTask DisposeAsync()
    {
        OrdersHubService.OrdersChanged -= OnOrdersChanged;
        _timeRefreshTimer.Dispose();

        await BrowserViewportService.UnsubscribeAsync(_viewportSubscriptionId);
    }

    private async Task OnViewportChangedAsync(BrowserViewportEventArgs args)
    {
        IsSplitView = args.Breakpoint is not Breakpoint.Xs;

        await InvokeAsync(StateHasChanged);
    }

    private void SelectOrder(OrderDto order)
    {
        if (IsSplitView)
        {
            SelectedOrderId = order.Id;
            return;
        }

        NavigationManager.NavigateTo($"/orders/{order.Id}");
    }

    private void ClearSelection()
    {
        SelectedOrderId = null;
    }

    // The loading flags start true and are never set again, so live updates swap the tickets in place
    private void OnOrdersChanged()
    {
        _ = InvokeAsync(GetTabsAsync);
    }

    private async Task GetTabsAsync()
    {
        await Task.WhenAll(
            GetQueueAsync(),
            GetMineAsync(),
            GetDoneAsync());
    }

    private async Task GetQueueAsync()
    {
        DateTimeOffset loadStartedAtUtc = DateTimeOffset.UtcNow;

        PaginationDto<OrderDto>? pagination = await GetOrdersAsync(
            OrderStatus.Pending,
            OrderSortBy.CreatedAtUtcAsc,
            bartenderId: null);

        QueueOrders = pagination?.Items ?? [];
        QueueCount = pagination?.TotalCount ?? 0;

        NewQueueOrders.Track(QueueOrders, loadStartedAtUtc, () => InvokeAsync(StateHasChanged));

        IsLoadingQueue = false;

        StateHasChanged();
    }

    private async Task GetMineAsync()
    {
        Task<PaginationDto<OrderDto>?> activeTask = GetOrdersAsync(
            OrderStatus.Active,
            OrderSortBy.CreatedAtUtcAsc,
            AuthService.UserId);

        Task<PaginationDto<OrderDto>?> acceptedTask = GetOrdersAsync(
            OrderStatus.Accepted,
            OrderSortBy.CreatedAtUtcAsc,
            AuthService.UserId);

        PaginationDto<OrderDto>?[] results = await Task.WhenAll(activeTask, acceptedTask);

        // What is being made comes first, then what is waiting to be started
        MyOrders = [.. results.SelectMany(pagination => pagination?.Items ?? [])];
        MineCount = results.Sum(pagination => pagination?.TotalCount ?? 0);

        IsLoadingMine = false;

        StateHasChanged();
    }

    private async Task GetDoneAsync()
    {
        Task<PaginationDto<OrderDto>?> completedTask = GetOrdersAsync(
            OrderStatus.Completed,
            OrderSortBy.CreatedAtUtcDesc,
            AuthService.UserId);

        Task<PaginationDto<OrderDto>?> declinedTask = GetOrdersAsync(
            OrderStatus.Declined,
            OrderSortBy.CreatedAtUtcDesc,
            AuthService.UserId);

        PaginationDto<OrderDto>?[] results = await Task.WhenAll(completedTask, declinedTask);

        DateTimeOffset todayStartUtc = TimeZoneService.TimeZone.GetTodayStartUtc();

        DoneOrders =
        [
            .. results
                .SelectMany(pagination => pagination?.Items ?? [])
                .Where(order => GetFinishedAtUtc(order) >= todayStartUtc)
                .OrderByDescending(GetFinishedAtUtc)
        ];

        IsLoadingDone = false;

        StateHasChanged();
    }

    private async Task<PaginationDto<OrderDto>?> GetOrdersAsync(
        OrderStatus status,
        OrderSortBy sortBy,
        Guid? bartenderId)
    {
        return await ApiService.ExecuteAsync(() => OrderApiClient.GetOrdersAsync(
            pageNumber: 1,
            MaxOrdersPerStatus,
            sortBy.ToString(),
            status,
            bartenderId));
    }

    private static DateTimeOffset? GetFinishedAtUtc(OrderDto order)
    {
        return order.CompletedAtUtc ?? order.DeclinedAtUtc;
    }

    private void SelectTab(OrdersTab tab)
    {
        OrdersTabState.Tab = tab;
    }

    private string GetTabStyle(OrdersTab tab)
    {
        return tab == OrdersTabState.Tab ? ActiveTabStyle : InactiveTabStyle;
    }

    // Nothing while the tab is loading
    private int? GetTabCount(OrdersTab tab)
    {
        return tab switch
        {
            OrdersTab.Queue => IsLoadingQueue ? null : QueueCount,
            OrdersTab.Mine => IsLoadingMine ? null : MineCount,
            OrdersTab.Done => IsLoadingDone ? null : DoneOrders.Count,
            _ => throw new ArgumentOutOfRangeException(nameof(tab), tab, null)
        };
    }

    private async Task RefreshTimesAsync()
    {
        while (await _timeRefreshTimer.WaitForNextTickAsync())
        {
            await InvokeAsync(StateHasChanged);
        }
    }
}
using OrderPoint.Customer.Orders.Api;
using OrderPoint.Customer.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;
using OrderPoint.Shared.Services;

namespace OrderPoint.Customer.Orders.Services;

// One per circuit: the live state of this phone's orders, for My orders, its badge and the status bar
internal sealed class MyOrdersStatusService(
    ApiService apiService,
    OrderApiClient orderApiClient,
    MyOrdersService myOrdersService,
    OrderTrackingHubService orderTrackingHubService) : IDisposable
{
    // A finished order stays in the status bar for a while, so the guest sees it was served or declined
    private static readonly TimeSpan FinishedOrderVisibility = TimeSpan.FromMinutes(10);

    private bool _isSubscribed;

    private int _reloadVersion;

    internal event Action? Changed;

    internal bool IsLoaded { get; private set; }

    // Newest first
    internal IReadOnlyList<OrderTrackingDto> Orders { get; private set; } = [];

    internal int OpenCount => Orders.Count(IsOpen);

    // The newest open order, else the newest one that finished a moment ago
    internal OrderTrackingDto? LatestOrder =>
        Orders.FirstOrDefault(IsOpen) ?? Orders.FirstOrDefault(IsRecentlyFinished);

    // The open orders besides the latest one
    internal int OtherOpenCount => LatestOrder is not null && IsOpen(LatestOrder) ? OpenCount - 1 : OpenCount;

    internal async Task LoadAsync()
    {
        if (!_isSubscribed)
        {
            orderTrackingHubService.OrdersChanged += OnOrdersChanged;
            myOrdersService.Changed += OnOrdersChanged;

            _isSubscribed = true;
        }

        await ReloadAsync();
    }

    // One call per order, in parallel; an older reload that answers last never overwrites a newer one
    internal async Task ReloadAsync()
    {
        int reloadVersion = Interlocked.Increment(ref _reloadVersion);

        OrderTrackingDto?[] orders = await Task.WhenAll(myOrdersService.OrderIds.Select(orderId =>
            apiService.ExecuteAsync(() => orderApiClient.GetOrderTrackingAsync(orderId))));

        if (reloadVersion != _reloadVersion)
        {
            return;
        }

        Orders = orders.OfType<OrderTrackingDto>().ToList();
        IsLoaded = true;

        Changed?.Invoke();
    }

    public void Dispose()
    {
        orderTrackingHubService.OrdersChanged -= OnOrdersChanged;
        myOrdersService.Changed -= OnOrdersChanged;
    }

    private void OnOrdersChanged()
    {
        _ = ReloadAsync();
    }

    private static bool IsOpen(OrderTrackingDto order)
    {
        return order.Status is OrderStatus.Pending or OrderStatus.Accepted or OrderStatus.Active;
    }

    private static bool IsRecentlyFinished(OrderTrackingDto order)
    {
        DateTimeOffset? finishedAtUtc = order.CompletedAtUtc ?? order.DeclinedAtUtc ?? order.CancelledAtUtc;

        return finishedAtUtc >= DateTimeOffset.UtcNow - FinishedOrderVisibility;
    }
}
using Microsoft.AspNetCore.SignalR.Client;

namespace OrderPoint.Customer.Orders.Services;

// One connection per circuit to the API's anonymous tracking hub, following this phone's orders. The layout starts
// it; placing an order adds the new one.
internal sealed partial class OrderTrackingHubService(
    IConfiguration configuration,
    ILogger<OrderTrackingHubService> logger) : IAsyncDisposable
{
    private const string ApiServiceName = "order-point-api";

    private const string HubPath = "hubs/order-tracking";

    private const string TrackOrderMethod = "TrackOrder";

    private const string OrderChangedMethod = "OrderChanged";

    // Keeps retrying every 30 s, so a long API restart doesn't leave the guest without updates
    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.Zero,
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(30)
    ];

    private readonly HashSet<Guid> _trackedOrderIds = [];

    private HubConnection? _connection;

    private CancellationTokenSource? _stopping;

    // One of the tracked orders changed (or the connection came back, and something may have); listeners reload
    internal event Action? OrdersChanged;

    internal async Task StartAsync()
    {
        if (_connection is not null)
        {
            return;
        }

        // WebSockets bypass the service discovery HttpClient, so the API URL comes from Aspire's configuration
        string? apiUrl = configuration[$"services:{ApiServiceName}:https:0"]
            ?? configuration[$"services:{ApiServiceName}:http:0"];

        if (apiUrl is null)
        {
            LogApiUrlMissing(ApiServiceName);

            return;
        }

        _connection = new HubConnectionBuilder()
            .WithUrl(new Uri(new Uri(apiUrl), HubPath))
            .WithAutomaticReconnect(new RetryPolicy())
            .Build();

        _connection.On(OrderChangedMethod, () => OrdersChanged?.Invoke());
        _connection.Reconnecting += OnReconnectingAsync;
        _connection.Reconnected += OnReconnectedAsync;

        _stopping = new CancellationTokenSource();

        await ConnectAsync(_connection, _stopping.Token);
    }

    // Before the connection is up the id is only remembered; connecting tracks every remembered id
    internal async Task TrackAsync(Guid orderId)
    {
        _trackedOrderIds.Add(orderId);

        if (_connection?.State == HubConnectionState.Connected)
        {
            await TrackOrderAsync(_connection, orderId);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is null)
        {
            return;
        }

        await _stopping!.CancelAsync();
        _stopping.Dispose();

        await _connection.DisposeAsync();
    }

    // Automatic reconnect only covers connections that were up once, so the first connect retries itself
    private async Task ConnectAsync(HubConnection connection, CancellationToken cancellationToken)
    {
        for (int attempt = 0; !cancellationToken.IsCancellationRequested; attempt++)
        {
            try
            {
                await Task.Delay(GetRetryDelay(attempt), cancellationToken);
                await connection.StartAsync(cancellationToken);
                await TrackAllAsync(connection);

                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                LogConnectFailed(exception);
            }
        }
    }

    private async Task TrackAllAsync(HubConnection connection)
    {
        foreach (Guid orderId in _trackedOrderIds.ToList())
        {
            await TrackOrderAsync(connection, orderId);
        }
    }

    private async Task TrackOrderAsync(HubConnection connection, Guid orderId)
    {
        try
        {
            await connection.InvokeAsync(TrackOrderMethod, orderId);
        }
        catch (Exception exception)
        {
            // Tracked again after the next reconnect
            LogTrackFailed(exception, orderId);
        }
    }

    private Task OnReconnectingAsync(Exception? exception)
    {
        LogConnectionLost(exception);

        return Task.CompletedTask;
    }

    // Groups don't survive a reconnect and changes sent meanwhile are lost, so track again and let listeners reload
    private async Task OnReconnectedAsync(string? connectionId)
    {
        await TrackAllAsync(_connection!);

        OrdersChanged?.Invoke();
    }

    private static TimeSpan GetRetryDelay(long attempt)
    {
        return RetryDelays[Math.Min(attempt, RetryDelays.Length - 1)];
    }

    private sealed class RetryPolicy : IRetryPolicy
    {
        public TimeSpan? NextRetryDelay(RetryContext retryContext)
        {
            return GetRetryDelay(retryContext.PreviousRetryCount);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "No URL configured for {ServiceName}; orders won't update live")]
    private partial void LogApiUrlMissing(string serviceName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Could not connect to the order tracking hub; retrying")]
    private partial void LogConnectFailed(Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Could not track order {OrderId}")]
    private partial void LogTrackFailed(Exception exception, Guid orderId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Lost the connection to the order tracking hub; reconnecting")]
    private partial void LogConnectionLost(Exception? exception);
}
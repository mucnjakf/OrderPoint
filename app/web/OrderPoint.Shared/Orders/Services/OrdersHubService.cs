using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderPoint.Shared.Auth.Services;
using OrderPoint.Shared.Errors;

namespace OrderPoint.Shared.Orders.Services;

// One connection per circuit; the layout starts it after sign-in and stops it on sign-out
internal sealed partial class OrdersHubService(
    IConfiguration configuration,
    AuthService authService,
    ILogger<OrdersHubService> logger) : IAsyncDisposable
{
    private const string ApiServiceName = "order-point-api";

    private const string HubPath = "hubs/orders";

    private const string OrdersChangedMethod = "OrdersChanged";

    private const string MenuChangedMethod = "MenuChanged";

    private const string TablesChangedMethod = "TablesChanged";

    // Keeps retrying every 30 s, so a long API restart doesn't leave the app without updates
    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.Zero,
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(30)
    ];

    private HubConnection? _connection;

    private CancellationTokenSource? _stopping;

    internal event Action? OrdersChanged;

    internal event Action? MenuChanged;

    internal event Action? TablesChanged;

    internal event Action? StateChanged;

    internal HubConnectionState State => _connection?.State ?? HubConnectionState.Disconnected;

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
            .WithUrl(new Uri(new Uri(apiUrl), HubPath), options => options.AccessTokenProvider = GetAccessTokenAsync)
            .WithAutomaticReconnect(new RetryPolicy())
            .Build();

        _connection.On(OrdersChangedMethod, () => OrdersChanged?.Invoke());
        _connection.On(MenuChangedMethod, () => MenuChanged?.Invoke());
        _connection.On(TablesChangedMethod, () => TablesChanged?.Invoke());
        _connection.Reconnecting += OnReconnectingAsync;
        _connection.Reconnected += OnReconnectedAsync;
        _connection.Closed += OnClosedAsync;

        _stopping = new CancellationTokenSource();

        await ConnectAsync(_connection, _stopping.Token);
    }

    internal async Task StopAsync()
    {
        if (_connection is null)
        {
            return;
        }

        HubConnection connection = _connection;
        _connection = null;

        await _stopping!.CancelAsync();
        _stopping.Dispose();
        _stopping = null;

        await connection.DisposeAsync();

        StateChanged?.Invoke();
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
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

                StateChanged?.Invoke();

                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (SessionExpiredException)
            {
                // Signed out; the layout stops the connection
                return;
            }
            catch (Exception exception)
            {
                LogConnectFailed(exception);
            }
        }
    }

    private async Task<string?> GetAccessTokenAsync()
    {
        return await authService.GetAccessTokenAsync();
    }

    private Task OnReconnectingAsync(Exception? exception)
    {
        LogConnectionLost(exception);
        StateChanged?.Invoke();

        return Task.CompletedTask;
    }

    // Changes sent while the connection was down are lost, so listeners reload
    private Task OnReconnectedAsync(string? connectionId)
    {
        StateChanged?.Invoke();
        OrdersChanged?.Invoke();
        MenuChanged?.Invoke();
        TablesChanged?.Invoke();

        return Task.CompletedTask;
    }

    private Task OnClosedAsync(Exception? exception)
    {
        StateChanged?.Invoke();

        return Task.CompletedTask;
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

    [LoggerMessage(Level = LogLevel.Warning, Message = "Could not connect to the orders hub; retrying")]
    private partial void LogConnectFailed(Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Lost the connection to the orders hub; reconnecting")]
    private partial void LogConnectionLost(Exception? exception);
}
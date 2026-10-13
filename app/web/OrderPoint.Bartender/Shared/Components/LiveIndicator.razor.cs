using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using OrderPoint.Shared.Orders.Services;

namespace OrderPoint.Bartender.Shared.Components;

// Whether orders update by themselves; offline means the list only changes after a reload
public sealed partial class LiveIndicator : IDisposable
{
    [Inject]
    private OrdersHubService OrdersHubService { get; set; } = null!;

    private Color StatusColor => OrdersHubService.State switch
    {
        HubConnectionState.Connected => Color.Success,
        HubConnectionState.Disconnected => Color.Error,
        _ => Color.Warning
    };

    private string StatusText => OrdersHubService.State switch
    {
        HubConnectionState.Connected => "Live",
        HubConnectionState.Disconnected => "Offline",
        _ => "Connecting"
    };

    private string TooltipText => OrdersHubService.State switch
    {
        HubConnectionState.Connected => "New orders and changes show up by themselves.",
        HubConnectionState.Disconnected => "Not connected. Orders don't update by themselves.",
        _ => "Connecting to live updates..."
    };

    protected override void OnInitialized()
    {
        OrdersHubService.StateChanged += OnStateChanged;
    }

    public void Dispose()
    {
        OrdersHubService.StateChanged -= OnStateChanged;
    }

    private void OnStateChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }
}
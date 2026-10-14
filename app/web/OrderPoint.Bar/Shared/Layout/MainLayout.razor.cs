using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Shared.Account.Services;
using OrderPoint.Shared.Auth.Services;
using OrderPoint.Shared.Layout;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Services;

namespace OrderPoint.Bar.Shared.Layout;

public sealed partial class MainLayout : IDisposable
{
    // Room for the fixed bottom navigation, so the last row can scroll above it
    private const string MainContentStyle = $"padding-bottom: {BottomNavigation.Height};";

    [Inject]
    private AuthService AuthService { get; set; } = null!;

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Inject]
    private DarkModeService DarkModeService { get; set; } = null!;

    [Inject]
    private AccountService AccountService { get; set; } = null!;

    [Inject]
    private OrdersHubService OrdersHubService { get; set; } = null!;

    private MudTheme Theme { get; } = AppTheme.Create();

    protected override async Task OnInitializedAsync()
    {
        AuthService.SessionChanged += OnSessionChanged;
        DarkModeService.Changed += OnDarkModeChanged;

        await Task.WhenAll(AuthService.LoadAsync(), DarkModeService.LoadAsync(), TimeZoneService.LoadAsync());

        if (AuthService.IsSignedIn)
        {
            _ = OrdersHubService.StartAsync();

            await AccountService.LoadAsync();
        }
    }

    public void Dispose()
    {
        AuthService.SessionChanged -= OnSessionChanged;
        DarkModeService.Changed -= OnDarkModeChanged;
    }

    private void OnSessionChanged()
    {
        _ = InvokeAsync(async () =>
        {
            if (AuthService.IsSignedIn)
            {
                _ = OrdersHubService.StartAsync();

                await AccountService.LoadAsync();
            }
            else
            {
                AccountService.Clear();

                await OrdersHubService.StopAsync();
            }

            StateHasChanged();
        });
    }

    private void OnDarkModeChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }
}
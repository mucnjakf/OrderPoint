using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Customer.Cart.Services;
using OrderPoint.Customer.Orders.Services;
using OrderPoint.Customer.Settings.Enumerations;
using OrderPoint.Customer.Settings.Services;
using OrderPoint.Customer.Tables.Services;
using OrderPoint.Shared.Layout;
using OrderPoint.Shared.Services;

namespace OrderPoint.Customer.Shared.Layout;

// No sign-in: guests are anonymous, and the table they sit at comes from the QR code
public sealed partial class MainLayout : IDisposable
{
    // Room for the fixed bottom navigation (and the status bar above it), so the last row can scroll above them
    private const string MainContentStyle = $"padding-bottom: {BottomNavigation.Height};";

    private const string MainContentWithStatusBarStyle =
        $"padding-bottom: calc({BottomNavigation.Height} + {OrderStatusBar.Height});";

    private const string OrdersPath = "orders";

    [Inject]
    private TableService TableService { get; set; } = null!;

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Inject]
    private AppearanceService AppearanceService { get; set; } = null!;

    [Inject]
    private CartService CartService { get; set; } = null!;

    [Inject]
    private MyOrdersService MyOrdersService { get; set; } = null!;

    [Inject]
    private OrderTrackingHubService OrderTrackingHubService { get; set; } = null!;

    [Inject]
    private MyOrdersStatusService MyOrdersStatusService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private MudThemeProvider ThemeProvider { get; set; } = null!;

    private MudTheme Theme { get; } = AppTheme.Create();

    private bool IsSystemDarkMode { get; set; }

    private bool IsDarkMode => AppearanceService.Mode switch
    {
        AppearanceMode.Light => false,
        AppearanceMode.Dark => true,
        _ => IsSystemDarkMode
    };

    // My orders and the order pages show the same thing in full
    private bool IsStatusBarVisible =>
        MyOrdersStatusService.LatestOrder is not null
        && !NavigationManager.ToBaseRelativePath(NavigationManager.Uri)
            .StartsWith(OrdersPath, StringComparison.Ordinal);

    private string ContentStyle => IsStatusBarVisible ? MainContentWithStatusBarStyle : MainContentStyle;

    protected override async Task OnInitializedAsync()
    {
        TableService.Changed += OnStateChanged;
        AppearanceService.Changed += OnStateChanged;
        MyOrdersStatusService.Changed += OnStateChanged;

        await Task.WhenAll(
            TableService.LoadAsync(),
            TimeZoneService.LoadAsync(),
            AppearanceService.LoadAsync(),
            CartService.LoadAsync(),
            MyOrdersService.LoadAsync());

        foreach (Guid orderId in MyOrdersService.OrderIds)
        {
            await OrderTrackingHubService.TrackAsync(orderId);
        }

        // Not awaited: the first connect retries in the background
        _ = OrderTrackingHubService.StartAsync();

        // Not awaited either, so the page shows while the orders load
        _ = MyOrdersStatusService.LoadAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        IsSystemDarkMode = await ThemeProvider.GetSystemDarkModeAsync();

        await ThemeProvider.WatchSystemDarkModeAsync(OnSystemDarkModeChangedAsync);

        StateHasChanged();
    }

    public void Dispose()
    {
        TableService.Changed -= OnStateChanged;
        AppearanceService.Changed -= OnStateChanged;
        MyOrdersStatusService.Changed -= OnStateChanged;
    }

    private Task OnSystemDarkModeChangedAsync(bool isDarkMode)
    {
        IsSystemDarkMode = isDarkMode;

        return InvokeAsync(StateHasChanged);
    }

    private void OnStateChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }
}
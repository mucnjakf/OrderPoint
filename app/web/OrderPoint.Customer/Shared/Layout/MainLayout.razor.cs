using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Customer.Settings.Enumerations;
using OrderPoint.Customer.Settings.Services;
using OrderPoint.Customer.Tables.Services;
using OrderPoint.Shared.Layout;
using OrderPoint.Shared.Services;

namespace OrderPoint.Customer.Shared.Layout;

// No sign-in: guests are anonymous, and the table they sit at comes from the QR code
public sealed partial class MainLayout : IDisposable
{
    // Room for the fixed bottom navigation, so the last row can scroll above it
    private const string MainContentStyle = $"padding-bottom: {BottomNavigation.Height};";

    [Inject]
    private TableService TableService { get; set; } = null!;

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Inject]
    private AppearanceService AppearanceService { get; set; } = null!;

    private MudThemeProvider ThemeProvider { get; set; } = null!;

    private MudTheme Theme { get; } = AppTheme.Create();

    private bool IsSystemDarkMode { get; set; }

    private bool IsDarkMode => AppearanceService.Mode switch
    {
        AppearanceMode.Light => false,
        AppearanceMode.Dark => true,
        _ => IsSystemDarkMode
    };

    protected override async Task OnInitializedAsync()
    {
        TableService.Changed += OnStateChanged;
        AppearanceService.Changed += OnStateChanged;

        await Task.WhenAll(TableService.LoadAsync(), TimeZoneService.LoadAsync(), AppearanceService.LoadAsync());
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
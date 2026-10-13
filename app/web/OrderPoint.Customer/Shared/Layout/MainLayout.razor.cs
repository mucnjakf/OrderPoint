using Microsoft.AspNetCore.Components;
using MudBlazor;
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

    private MudThemeProvider ThemeProvider { get; set; } = null!;

    private MudTheme Theme { get; } = AppTheme.Create();

    // Guests have no settings page, so the app follows the phone's light or dark mode
    private bool IsDarkMode { get; set; }

    protected override async Task OnInitializedAsync()
    {
        TableService.Changed += OnTableChanged;

        await Task.WhenAll(TableService.LoadAsync(), TimeZoneService.LoadAsync());
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        IsDarkMode = await ThemeProvider.GetSystemDarkModeAsync();

        await ThemeProvider.WatchSystemDarkModeAsync(OnSystemDarkModeChangedAsync);

        StateHasChanged();
    }

    public void Dispose()
    {
        TableService.Changed -= OnTableChanged;
    }

    private Task OnSystemDarkModeChangedAsync(bool isDarkMode)
    {
        IsDarkMode = isDarkMode;

        return InvokeAsync(StateHasChanged);
    }

    private void OnTableChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }
}
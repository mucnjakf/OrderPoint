using System.Security.Cryptography;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using MudBlazor;
using OrderPoint.Admin.Auth.Services;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Shared.Layout;

public sealed partial class MainLayout : IDisposable
{
    private const string DarkModeStorageKey = "dark-mode";

    private const string DrawerOpenStorageKey = "drawer-open";

    private const string DrawerToggleWrapperStyle =
        "position: fixed; left: 0; bottom: 92px; transform: translateX(-50%); " +
        "z-index: calc(var(--mud-zindex-drawer) + 1)";

    private const string DrawerToggleButtonStyle =
        "background-color: var(--mud-palette-surface); border: 1px solid var(--mud-palette-lines-default)";

    [Inject]
    private AuthService AuthService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private ProtectedLocalStorage ProtectedLocalStorage { get; set; } = null!;

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    private MudTheme Theme { get; } = AdminTheme.Create();

    private bool DrawerOpen { get; set; } = true;

    private bool IsDarkMode { get; set; } = true;

    private string DarkModeIcon => IsDarkMode ? Icons.Material.Filled.LightMode : Icons.Material.Filled.DarkMode;

    private string DrawerToggleIcon => DrawerOpen
        ? Icons.Material.Filled.ChevronLeft
        : Icons.Material.Filled.ChevronRight;

    protected override async Task OnInitializedAsync()
    {
        AuthService.SessionChanged += OnSessionChanged;

        await Task.WhenAll(LoadLayoutPreferencesAsync(), AuthService.LoadAsync(), TimeZoneService.LoadAsync());
    }

    public void Dispose()
    {
        AuthService.SessionChanged -= OnSessionChanged;
    }

    private void OnSessionChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }

    private async Task LoadLayoutPreferencesAsync()
    {
        IsDarkMode = await LoadPreferenceAsync(DarkModeStorageKey, defaultValue: true);
        DrawerOpen = await LoadPreferenceAsync(DrawerOpenStorageKey, defaultValue: true);
    }

    private async Task<bool> LoadPreferenceAsync(string storageKey, bool defaultValue)
    {
        try
        {
            ProtectedBrowserStorageResult<bool> result = await ProtectedLocalStorage.GetAsync<bool>(storageKey);

            return result.Success ? result.Value : defaultValue;
        }
        catch (CryptographicException)
        {
            await ProtectedLocalStorage.DeleteAsync(storageKey);

            return defaultValue;
        }
    }

    private async Task ToggleDrawerAsync()
    {
        DrawerOpen = !DrawerOpen;

        await ProtectedLocalStorage.SetAsync(DrawerOpenStorageKey, DrawerOpen);
    }

    private async Task ToggleDarkModeAsync()
    {
        IsDarkMode = !IsDarkMode;

        await ProtectedLocalStorage.SetAsync(DarkModeStorageKey, IsDarkMode);
    }

    private async Task SignOutAsync()
    {
        await ApiService.ExecuteAsync(() => AuthService.LogoutAsync(), "Signed out");
    }
}
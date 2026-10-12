using System.Security.Cryptography;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using MudBlazor;
using OrderPoint.Admin.Account.Services;
using OrderPoint.Admin.Auth.Services;
using OrderPoint.Admin.Health.Services;
using OrderPoint.Admin.Search.Dialogs;
using OrderPoint.Admin.Search.Dtos;
using OrderPoint.Admin.Search.Services;
using OrderPoint.Shared.Layout;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Shared.Layout;

public sealed partial class MainLayout : IDisposable
{
    private const string DarkModeStorageKey = "dark-mode";

    private const string DrawerOpenStorageKey = "drawer-open";

    private const int HealthCheckIntervalInSeconds = 30;

    private const string DrawerToggleWrapperStyle =
        "position: fixed; left: 0; bottom: 72px; transform: translateX(-50%); " +
        "z-index: calc(var(--mud-zindex-drawer) + 1)";

    private const string DrawerToggleButtonStyle =
        "background-color: var(--mud-palette-surface); border: 1px solid var(--mud-palette-lines-default)";

    private readonly PeriodicTimer _healthCheckTimer = new(TimeSpan.FromSeconds(HealthCheckIntervalInSeconds));

    [Inject]
    private AuthService AuthService { get; set; } = null!;

    [Inject]
    private ProtectedLocalStorage ProtectedLocalStorage { get; set; } = null!;

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private SearchService SearchService { get; set; } = null!;

    [Inject]
    private AccountService AccountService { get; set; } = null!;

    [Inject]
    private HealthService HealthService { get; set; } = null!;

    private DotNetObjectReference<MainLayout>? SelfReference { get; set; }

    private bool IsSearchOpen { get; set; }

    private MudTheme Theme { get; } = AppTheme.Create();

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

        _ = RefreshHealthAsync();

        if (AuthService.IsSignedIn)
        {
            await Task.WhenAll(AccountService.LoadAsync(), HealthService.CheckAsync());
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        SelfReference = DotNetObjectReference.Create(this);

        await JsRuntime.InvokeVoidAsync("registerSearchShortcut", SelfReference);
    }

    public void Dispose()
    {
        AuthService.SessionChanged -= OnSessionChanged;
        SelfReference?.Dispose();
        _healthCheckTimer.Dispose();
    }

    [JSInvokable]
    public async Task OpenSearchFromShortcutAsync()
    {
        await OpenSearchAsync();
    }

    private async Task OpenSearchAsync()
    {
        if (!AuthService.IsSignedIn || IsSearchOpen)
        {
            return;
        }

        IsSearchOpen = true;

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true,
            NoHeader = true,
            Position = DialogPosition.TopCenter
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<GlobalSearchDialog>(string.Empty, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        IsSearchOpen = false;

        if (dialogResult.Canceled)
        {
            return;
        }

        var result = (dialogResult.Data as SearchResultDto)!;

        await SearchService.OpenAsync(result);
    }

    private void OnSessionChanged()
    {
        _ = InvokeAsync(async () =>
        {
            if (AuthService.IsSignedIn)
            {
                await Task.WhenAll(AccountService.LoadAsync(), HealthService.CheckAsync());
            }
            else
            {
                AccountService.Clear();
            }

            StateHasChanged();
        });
    }

    private async Task RefreshHealthAsync()
    {
        while (await _healthCheckTimer.WaitForNextTickAsync())
        {
            if (AuthService.IsSignedIn)
            {
                await InvokeAsync(HealthService.CheckAsync);
            }
        }
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
}
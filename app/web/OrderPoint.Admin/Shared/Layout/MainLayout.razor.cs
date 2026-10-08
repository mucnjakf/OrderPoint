using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Auth.Services;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Shared.Layout;

public sealed partial class MainLayout : IDisposable
{
    [Inject]
    private AuthService AuthService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    private MudTheme Theme { get; } = new()
    {
        PaletteDark = new PaletteDark
        {
            Background = "#32333d",
            LinesDefault = "#4e4e4e"
        }
    };

    private bool DrawerOpen { get; set; } = true;

    private bool IsDarkMode { get; set; } = true;

    private string DarkModeIcon => IsDarkMode ? Icons.Material.Filled.LightMode : Icons.Material.Filled.DarkMode;

    protected override async Task OnInitializedAsync()
    {
        AuthService.SessionChanged += OnSessionChanged;

        await AuthService.LoadAsync();
    }

    public void Dispose()
    {
        AuthService.SessionChanged -= OnSessionChanged;
    }

    private void OnSessionChanged()
    {
        // Raised on sign in, sign out and when a session expires during an API call (possibly off the UI thread)
        _ = InvokeAsync(StateHasChanged);
    }

    private void ToggleDrawer()
    {
        DrawerOpen = !DrawerOpen;
    }

    private void ToggleDarkMode()
    {
        IsDarkMode = !IsDarkMode;
    }

    private async Task SignOutAsync()
    {
        await ApiService.ExecuteAsync(() => AuthService.LogoutAsync(), "Signed out");
    }
}
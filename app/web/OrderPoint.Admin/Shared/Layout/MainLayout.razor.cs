using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Auth.Services;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Shared.Layout;

public sealed partial class MainLayout : IDisposable
{
    private const string DrawerToggleWrapperStyle =
        "position: fixed; left: 0; bottom: 92px; transform: translateX(-50%); " +
        "z-index: calc(var(--mud-zindex-drawer) + 1)";

    private const string DrawerToggleButtonStyle =
        "background-color: var(--mud-palette-surface); border: 1px solid var(--mud-palette-lines-default)";

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

    private string DrawerToggleIcon => DrawerOpen
        ? Icons.Material.Filled.ChevronLeft
        : Icons.Material.Filled.ChevronRight;

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
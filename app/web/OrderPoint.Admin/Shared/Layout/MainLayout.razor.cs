using MudBlazor;

namespace OrderPoint.Admin.Shared.Layout;

public sealed partial class MainLayout
{
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

    private string IconButton { get; set; } = Icons.Material.Filled.LightMode;

    private void ToggleDrawer()
    {
        DrawerOpen = !DrawerOpen;
    }

    private void ToggleDarkMode()
    {
        IsDarkMode = !IsDarkMode;

        if (IsDarkMode)
        {
            IconButton = Icons.Material.Filled.LightMode;
            return;
        }

        IconButton = Icons.Material.Filled.DarkMode;
    }
}
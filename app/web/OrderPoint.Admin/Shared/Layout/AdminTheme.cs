using MudBlazor;

namespace OrderPoint.Admin.Shared.Layout;

internal static class AdminTheme
{
    private const string Primary = "#3b82f6";
    private const string Destructive = "#ef4444";
    private const string BorderRadius = "6px";

    private const string ShadowSmall =
        "0 1px 3px 0px hsl(0 0% 0% / 0.10), 0 1px 2px -1px hsl(0 0% 0% / 0.10)";

    private const string ShadowMedium =
        "0 1px 3px 0px hsl(0 0% 0% / 0.10), 0 2px 4px -1px hsl(0 0% 0% / 0.10)";

    private const string ShadowLarge =
        "0 1px 3px 0px hsl(0 0% 0% / 0.10), 0 4px 6px -1px hsl(0 0% 0% / 0.10)";

    private const string ShadowExtraLarge =
        "0 1px 3px 0px hsl(0 0% 0% / 0.10), 0 8px 10px -1px hsl(0 0% 0% / 0.10)";

    internal static MudTheme Create()
    {
        MudTheme theme = new()
        {
            PaletteLight = new PaletteLight
            {
                Primary = Primary,
                Error = Destructive,
                Background = "#f9fafb",
                BackgroundGray = "#f3f4f6",
                Surface = "#ffffff",
                AppbarBackground = "#ffffff",
                AppbarText = "#333333",
                DrawerBackground = "#f9fafb",
                DrawerText = "#333333",
                DrawerIcon = "#6b7280",
                TextPrimary = "#333333",
                TextSecondary = "#6b7280",
                LinesDefault = "#e5e7eb",
                LinesInputs = "#e5e7eb",
                TableLines = "#e5e7eb",
                Divider = "#e5e7eb"
            },
            PaletteDark = new PaletteDark
            {
                Primary = Primary,
                Error = Destructive,
                Background = "#171717",
                BackgroundGray = "#1f1f1f",
                Surface = "#262626",
                AppbarBackground = "#171717",
                AppbarText = "#e5e5e5",
                DrawerBackground = "#171717",
                DrawerText = "#e5e5e5",
                DrawerIcon = "#a3a3a3",
                TextPrimary = "#e5e5e5",
                TextSecondary = "#a3a3a3",
                LinesDefault = "#404040",
                LinesInputs = "#404040",
                TableLines = "#404040",
                Divider = "#404040"
            }
        };

        theme.Typography.Default.FontFamily = ["Inter", "sans-serif"];
        theme.Typography.Button.TextTransform = "none";
        theme.LayoutProperties.DefaultBorderRadius = BorderRadius;

        for (int elevation = 1; elevation < theme.Shadows.Elevation.Length; elevation++)
        {
            theme.Shadows.Elevation[elevation] = elevation switch
            {
                <= 2 => ShadowSmall,
                <= 4 => ShadowMedium,
                <= 8 => ShadowLarge,
                _ => ShadowExtraLarge
            };
        }

        return theme;
    }
}
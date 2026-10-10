using MudBlazor;

namespace OrderPoint.Admin.Shared.Layout;

internal static class AdminTheme
{
    private const string BorderRadius = "8px";
    private const string NoShadow = "none";

    internal static MudTheme Create()
    {
        MudTheme theme = new()
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#000000",
                PrimaryContrastText = "#ffffff",
                PrimaryDarken = "#1a1a1a",
                Error = "#e54b4f",
                Background = "#fcfcfc",
                BackgroundGray = "#f5f5f5",
                Surface = "#ffffff",
                AppbarBackground = "#ffffff",
                AppbarText = "#000000",
                DrawerBackground = "#fcfcfc",
                DrawerText = "#000000",
                DrawerIcon = "#525252",
                TextPrimary = "#000000",
                TextSecondary = "#525252",
                LinesDefault = "#e4e4e4",
                LinesInputs = "#ebebeb",
                TableLines = "#e4e4e4",
                TableHover = "#f0f0f0",
                HoverOpacity = 0.06,
                Divider = "#e4e4e4"
            },
            PaletteDark = new PaletteDark
            {
                Primary = "#ffffff",
                PrimaryContrastText = "#000000",
                PrimaryDarken = "#e6e6e6",
                Error = "#ff5b5b",
                Background = "#000000",
                BackgroundGray = "#1d1d1d",
                Surface = "#090909",
                AppbarBackground = "#000000",
                AppbarText = "#ffffff",
                DrawerBackground = "#121212",
                DrawerText = "#ffffff",
                DrawerIcon = "#a4a4a4",
                TextPrimary = "#ffffff",
                TextSecondary = "#a4a4a4",
                LinesDefault = "#242424",
                LinesInputs = "#333333",
                TableLines = "#242424",
                TableHover = "#222222",
                HoverOpacity = 0.13,
                Divider = "#242424"
            }
        };

        theme.Typography.Default.FontFamily = ["Geist", "sans-serif"];
        theme.Typography.Button.TextTransform = "none";
        theme.LayoutProperties.DefaultBorderRadius = BorderRadius;

        Array.Fill(theme.Shadows.Elevation, NoShadow);

        return theme;
    }
}
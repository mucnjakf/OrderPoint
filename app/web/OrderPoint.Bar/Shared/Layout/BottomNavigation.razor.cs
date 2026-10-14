using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using MudBlazor;

namespace OrderPoint.Bar.Shared.Layout;

// Fixed to the bottom of the screen, within thumb reach on a phone
public sealed partial class BottomNavigation : IDisposable
{
    internal const string Height = "64px";

    private const string NavigationStyle =
        $"position: fixed; left: 0; right: 0; bottom: 0; display: flex; padding: 4px 8px; " +
        $"height: {Height}; z-index: var(--mud-zindex-appbar); " +
        "background-color: var(--mud-palette-surface); border-top: 1px solid var(--mud-palette-lines-default);";

    private const string ActiveButtonStyle = "text-transform: none; font-weight: 600;";

    private const string InactiveButtonStyle = "text-transform: none; color: var(--mud-palette-text-secondary);";

    private static readonly (string Href, string Label, string Icon)[] Destinations =
    [
        ("/", "Orders", Icons.Material.Filled.Receipt),
        ("/tables", "Tables", Icons.Material.Filled.TableRestaurant),
        ("/shift", "My shift", Icons.Material.Filled.Schedule),
        ("/menu", "Menu", Icons.Material.Filled.MenuBook),
        ("/account", "Account", Icons.Material.Filled.Person)
    ];

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
    }

    // Orders lives at "/" and its details below "/orders", so it matches both
    private bool IsActive(string href)
    {
        string path = "/" + NavigationManager.ToBaseRelativePath(NavigationManager.Uri).Split('?')[0];

        return href == "/"
            ? path == "/" || path.StartsWith("/orders", StringComparison.Ordinal)
            : path.StartsWith(href, StringComparison.Ordinal);
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        StateHasChanged();
    }
}
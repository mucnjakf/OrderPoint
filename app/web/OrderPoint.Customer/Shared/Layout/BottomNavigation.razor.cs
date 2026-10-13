using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using MudBlazor;
using OrderPoint.Customer.Cart.Services;

namespace OrderPoint.Customer.Shared.Layout;

// Fixed to the bottom of the screen, within thumb reach on a phone
public sealed partial class BottomNavigation : IDisposable
{
    internal const string Height = "64px";

    private const string NavigationStyle =
        $"position: fixed; left: 0; right: 0; bottom: 0; display: flex; padding: 4px 8px; " +
        $"height: {Height}; z-index: var(--mud-zindex-appbar); " +
        "background-color: var(--mud-palette-surface); border-top: 1px solid var(--mud-palette-lines-default);";

    private const string CartHref = "/cart";

    // Visible overflow, so the cart badge above the icon isn't cut off by the button
    private const string ActiveButtonStyle = "text-transform: none; font-weight: 600; overflow: visible;";

    private const string InactiveButtonStyle =
        "text-transform: none; color: var(--mud-palette-text-secondary); overflow: visible;";

    private static readonly (string Href, string Label, string Icon)[] Destinations =
    [
        ("/", "Menu", Icons.Material.Filled.MenuBook),
        (CartHref, "Cart", Icons.Material.Filled.ShoppingCart),
        ("/orders", "My orders", Icons.Material.Filled.Receipt),
        ("/settings", "Settings", Icons.Material.Filled.Settings)
    ];

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private CartService CartService { get; set; } = null!;

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
        CartService.Changed += OnCartChanged;
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
        CartService.Changed -= OnCartChanged;
    }

    private void OnCartChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }

    // The menu lives at "/" and its categories and items below "/menu", so it matches both
    private bool IsActive(string href)
    {
        string path = "/" + NavigationManager.ToBaseRelativePath(NavigationManager.Uri).Split('?')[0];

        return href == "/"
            ? path == "/" || path.StartsWith("/menu", StringComparison.Ordinal)
            : path.StartsWith(href, StringComparison.Ordinal);
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        StateHasChanged();
    }
}
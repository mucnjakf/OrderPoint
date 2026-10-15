using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using MudBlazor;
using OrderPoint.Customer.Cart.Services;
using OrderPoint.Customer.Orders.Services;

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

    private const string OrdersHref = "/orders";

    private const string ActiveButtonStyle = "text-transform: none; font-weight: 600;";

    private const string InactiveButtonStyle = "text-transform: none; color: var(--mud-palette-text-secondary);";

    private static readonly (string Href, string Label, string Icon)[] Destinations =
    [
        ("/", "Menu", Icons.Material.Filled.MenuBook),
        (CartHref, "Cart", Icons.Material.Filled.ShoppingCart),
        (OrdersHref, "Orders", Icons.Material.Filled.Receipt),
        ("/visit", "Visit", Icons.Material.Filled.TableRestaurant)
    ];

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private CartService CartService { get; set; } = null!;

    [Inject]
    private MyOrdersStatusService MyOrdersStatusService { get; set; } = null!;

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
        CartService.Changed += OnBadgeChanged;
        MyOrdersStatusService.Changed += OnBadgeChanged;
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
        CartService.Changed -= OnBadgeChanged;
        MyOrdersStatusService.Changed -= OnBadgeChanged;
    }

    private void OnBadgeChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }

    // Pieces in the cart, and orders still on their way
    private int GetBadgeCount(string href)
    {
        return href switch
        {
            CartHref => CartService.ItemsCount,
            OrdersHref => MyOrdersStatusService.OpenCount,
            _ => 0
        };
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
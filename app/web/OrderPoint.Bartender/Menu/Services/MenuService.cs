using OrderPoint.Shared.Menu.Api;
using OrderPoint.Shared.Menu.Dtos;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Services;

namespace OrderPoint.Bartender.Menu.Services;

// One per circuit: the small menu is loaded once for every order page, and again after it changes (Changed)
internal sealed class MenuService(
    ApiService apiService,
    MenuApiClient menuApiClient,
    OrdersHubService ordersHubService) : IDisposable
{
    private Task<IReadOnlyList<MenuCategoryDto>?>? _menuTask;

    private bool _isSubscribed;

    internal event Action? Changed;

    internal async Task<IReadOnlyList<MenuCategoryDto>?> GetMenuAsync()
    {
        if (!_isSubscribed)
        {
            ordersHubService.MenuChanged += OnMenuChanged;

            _isSubscribed = true;
        }

        _menuTask ??= apiService.ExecuteAsync(() => menuApiClient.GetMenuAsync());

        IReadOnlyList<MenuCategoryDto>? menu = await _menuTask;

        // A failed load is tried again by the next page that asks
        if (menu is null)
        {
            _menuTask = null;
        }

        return menu;
    }

    // The next page that asks loads the menu again
    internal void Reset()
    {
        _menuTask = null;
    }

    public void Dispose()
    {
        ordersHubService.MenuChanged -= OnMenuChanged;
    }

    private void OnMenuChanged()
    {
        _menuTask = null;

        Changed?.Invoke();
    }
}
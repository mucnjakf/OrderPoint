using OrderPoint.Customer.Menu.Api;
using OrderPoint.Customer.Menu.Dtos;
using OrderPoint.Shared.Services;

namespace OrderPoint.Customer.Menu.Services;

// One per circuit: the whole menu is small, so it is loaded once per visit and every menu page reads it from here
internal sealed class MenuService(ApiService apiService, MenuApiClient menuApiClient)
{
    private Task<IReadOnlyList<MenuCategoryDto>?>? _menuTask;

    internal async Task<IReadOnlyList<MenuCategoryDto>?> GetMenuAsync()
    {
        _menuTask ??= apiService.ExecuteAsync(() => menuApiClient.GetMenuAsync());

        IReadOnlyList<MenuCategoryDto>? menu = await _menuTask;

        // A failed load is tried again by the next page that asks
        if (menu is null)
        {
            _menuTask = null;
        }

        return menu;
    }
}
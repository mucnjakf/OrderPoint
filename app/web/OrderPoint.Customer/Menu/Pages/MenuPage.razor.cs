using Microsoft.AspNetCore.Components;
using OrderPoint.Customer.Menu.Dtos;
using OrderPoint.Customer.Menu.Services;
using OrderPoint.Customer.Tables.Services;

namespace OrderPoint.Customer.Menu.Pages;

// The categories in menu order; searching shows matching items from every category instead
public sealed partial class MenuPage
{
    // Three rows of two cards on a phone
    private const int LoadingCardsCount = 6;

    // The search stays right under the dense app bar while scrolling; the menu is already loaded, so it filters
    // as the guest types
    private const string SearchWrapperStyle =
        "position: sticky; top: 48px; z-index: 2; padding: 12px 0; margin-top: -12px; " +
        "background-color: var(--mud-palette-background);";

    [Inject]
    private MenuService MenuService { get; set; } = null!;

    [Inject]
    private TableService TableService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private IReadOnlyList<MenuCategoryDto> Categories { get; set; } = [];

    private bool IsLoading { get; set; } = true;

    private string? SearchQuery { get; set; }

    private bool IsSearching => !string.IsNullOrWhiteSpace(SearchQuery);

    private string Subtitle => TableService.TableCode is null
        ? "What would you like?"
        : $"Table {TableService.TableCode} · What would you like?";

    private IEnumerable<(MenuCategoryDto Category, MenuItemDto Item)> FoundItems => Categories
        .SelectMany(category => category.Items, (category, item) => (category, item))
        .Where(found => found.item.Name.Contains(SearchQuery!.Trim(), StringComparison.CurrentCultureIgnoreCase))
        .OrderBy(found => found.item.Name);

    protected override async Task OnInitializedAsync()
    {
        Categories = await MenuService.GetMenuAsync() ?? [];

        IsLoading = false;
    }

    private void OpenCategory(MenuCategoryDto category)
    {
        NavigationManager.NavigateTo($"/menu/{category.Id}");
    }

    private void OpenItem(MenuCategoryDto category, MenuItemDto item)
    {
        NavigationManager.NavigateTo($"/menu/{category.Id}/items/{item.Id}");
    }
}
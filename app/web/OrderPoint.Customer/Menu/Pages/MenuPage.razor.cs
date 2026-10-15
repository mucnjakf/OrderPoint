using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Customer.Menu.Services;
using OrderPoint.Customer.Tables.Services;
using OrderPoint.Shared.Menu.Dtos;

namespace OrderPoint.Customer.Menu.Pages;

// The categories in menu order; a category opens its items
public sealed partial class MenuPage : IDisposable
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

    private IEnumerable<MenuCategoryDto> FoundCategories => IsSearching
        ? Categories.Where(category =>
            category.Name.Contains(SearchQuery!.Trim(), StringComparison.CurrentCultureIgnoreCase))
        : Categories;

    private string EmptyStateIcon => IsSearching ? Icons.Material.Filled.SearchOff : Icons.Material.Filled.MenuBook;

    private string EmptyStateText => IsSearching ? "Nothing found" : "The menu is empty";

    private string EmptyStateDetail => IsSearching
        ? "Try another name."
        : "Please ask the staff what they can serve.";

    protected override async Task OnInitializedAsync()
    {
        MenuService.Changed += OnMenuChanged;

        Categories = await MenuService.GetMenuAsync() ?? [];

        IsLoading = false;
    }

    public void Dispose()
    {
        MenuService.Changed -= OnMenuChanged;
    }

    // A failed reload keeps what is shown
    private void OnMenuChanged()
    {
        _ = InvokeAsync(async () =>
        {
            IReadOnlyList<MenuCategoryDto>? menu = await MenuService.GetMenuAsync();

            if (menu is null)
            {
                return;
            }

            Categories = menu;

            StateHasChanged();
        });
    }

    private void OpenCategory(MenuCategoryDto category)
    {
        NavigationManager.NavigateTo($"/menu/{category.Id}");
    }
}
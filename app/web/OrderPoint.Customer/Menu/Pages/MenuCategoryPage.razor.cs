using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Customer.Cart.Services;
using OrderPoint.Customer.Menu.Dtos;
using OrderPoint.Customer.Menu.Services;

namespace OrderPoint.Customer.Menu.Pages;

public sealed partial class MenuCategoryPage : IDisposable
{
    private const int LoadingRowsCount = 3;

    // The search stays right under the dense app bar while scrolling; the menu is already loaded, so it filters
    // as the guest types
    private const string SearchWrapperStyle =
        "position: sticky; top: 48px; z-index: 2; padding: 12px 0; margin-top: -12px; " +
        "background-color: var(--mud-palette-background);";

    [Inject]
    private MenuService MenuService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private CartService CartService { get; set; } = null!;

    [Parameter]
    public Guid CategoryId { get; set; }

    private MenuCategoryDto? Category { get; set; }

    private bool IsLoading { get; set; } = true;

    private string? SearchQuery { get; set; }

    private IEnumerable<MenuItemDto> FoundItems => string.IsNullOrWhiteSpace(SearchQuery)
        ? Category?.Items ?? []
        : (Category?.Items ?? [])
            .Where(item => item.Name.Contains(SearchQuery.Trim(), StringComparison.CurrentCultureIgnoreCase));

    // Taken off the menu since the guest opened it, or a link from an old visit
    private string EmptyStateIcon => Category is null
        ? Icons.Material.Filled.MenuBook
        : Icons.Material.Filled.SearchOff;

    private string EmptyStateText => Category is null ? "Category not found" : "Nothing found";

    private string EmptyStateDetail => Category is null
        ? "It may have been taken off the menu."
        : "Try another name.";

    protected override async Task OnInitializedAsync()
    {
        CartService.Changed += OnCartChanged;
        MenuService.Changed += OnMenuChanged;

        IReadOnlyList<MenuCategoryDto> menu = await MenuService.GetMenuAsync() ?? [];

        Category = menu.FirstOrDefault(category => category.Id == CategoryId);

        IsLoading = false;
    }

    public void Dispose()
    {
        CartService.Changed -= OnCartChanged;
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

            Category = menu.FirstOrDefault(category => category.Id == CategoryId);

            StateHasChanged();
        });
    }

    private void OpenItem(MenuItemDto item)
    {
        NavigationManager.NavigateTo($"/menu/{CategoryId}/items/{item.Id}");
    }

    private async Task AddToCartAsync(MenuItemDto item)
    {
        await CartService.AddAsync(item.Id, CategoryId, item.Name, quantity: 1);
    }

    private void OnCartChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }
}
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Bartender.Menu.Services;
using OrderPoint.Bartender.Orders.Services;
using OrderPoint.Bartender.Shared.Extensions;
using OrderPoint.Shared.Menu.Dtos;

namespace OrderPoint.Bartender.Orders.Pages;

// A category's items: + adds one, a tap opens the item
public sealed partial class OrderCategoryPage : IDisposable
{
    // The search stays right under the dense app bar while scrolling; the menu is already loaded, so it filters
    // as the bartender types
    private const string SearchWrapperStyle =
        "position: sticky; top: 48px; z-index: 2; padding: 12px 0; margin-top: -12px; " +
        "background-color: var(--mud-palette-background);";

    private const int LoadingRowsCount = 3;

    [Inject]
    private MenuService MenuService { get; set; } = null!;

    [Inject]
    private OrderDraftService OrderDraftService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    // Null at the counter
    [Parameter]
    public string? TableCode { get; set; }

    [Parameter]
    public Guid CategoryId { get; set; }

    private IReadOnlyList<MenuCategoryDto> Menu { get; set; } = [];

    private bool IsLoading { get; set; } = true;

    private string? SearchQuery { get; set; }

    private MenuCategoryDto? Category => Menu.FirstOrDefault(category => category.Id == CategoryId);

    private OrderDraft Draft => OrderDraftService.Get(TableCode);

    private string PlaceText => TableCode.ToPlaceText();

    private string OrderPadUri => TableCode.ToOrderPadUri();

    private string ReviewUri => $"{OrderPadUri}/review";

    private IEnumerable<MenuItemDto> FoundItems => string.IsNullOrWhiteSpace(SearchQuery)
        ? Category?.Items ?? []
        : (Category?.Items ?? [])
            .Where(item => item.Name.Contains(SearchQuery.Trim(), StringComparison.CurrentCultureIgnoreCase));

    // Taken off the menu since the bartender opened it
    private string EmptyStateIcon => Category is null
        ? Icons.Material.Filled.MenuBook
        : Icons.Material.Filled.SearchOff;

    private string EmptyStateText => Category is null ? "Category not found" : "Nothing found";

    private string EmptyStateDetail => Category is null
        ? "It may have been taken off the menu."
        : "Try another name.";

    protected override async Task OnInitializedAsync()
    {
        MenuService.Changed += OnMenuChanged;

        Menu = await MenuService.GetMenuAsync() ?? [];

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

            Menu = menu;

            StateHasChanged();
        });
    }

    private void OpenItem(MenuItemDto item)
    {
        NavigationManager.NavigateTo($"{OrderPadUri}/{CategoryId}/items/{item.Id}");
    }

    private void AddItem(MenuItemDto item)
    {
        Draft.SetQuantity(item.Id, item.Name, Draft.GetQuantity(item.Id) + 1);
    }

    private void SetQuantity(MenuItemDto item, int quantity)
    {
        Draft.SetQuantity(item.Id, item.Name, quantity);
    }
}
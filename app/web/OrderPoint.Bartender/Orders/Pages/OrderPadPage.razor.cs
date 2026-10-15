using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Bartender.Menu.Services;
using OrderPoint.Bartender.Orders.Services;
using OrderPoint.Bartender.Shared.Extensions;
using OrderPoint.Shared.Menu.Dtos;

namespace OrderPoint.Bartender.Orders.Pages;

// The categories of a new order; the order stays a draft while the bartender looks around
public sealed partial class OrderPadPage : IDisposable
{
    // The search stays right under the dense app bar while scrolling; the menu is already loaded, so it filters
    // as the bartender types
    private const string SearchWrapperStyle =
        "position: sticky; top: 48px; z-index: 2; padding: 12px 0; margin-top: -12px; " +
        "background-color: var(--mud-palette-background);";

    private const int LoadingCardsCount = 6;

    [Inject]
    private MenuService MenuService { get; set; } = null!;

    [Inject]
    private OrderDraftService OrderDraftService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    // Null at the counter
    [Parameter]
    public string? TableCode { get; set; }

    private IReadOnlyList<MenuCategoryDto> Menu { get; set; } = [];

    private bool IsLoading { get; set; } = true;

    private string? SearchQuery { get; set; }

    private bool IsSearching => !string.IsNullOrWhiteSpace(SearchQuery);

    private OrderDraft Draft => OrderDraftService.Get(TableCode);

    private string PlaceText => TableCode.ToPlaceText();

    private string ReviewUri => $"{TableCode.ToOrderPadUri()}/review";

    private IEnumerable<MenuCategoryDto> FoundCategories => IsSearching
        ? Menu.Where(category =>
            category.Name.Contains(SearchQuery!.Trim(), StringComparison.CurrentCultureIgnoreCase))
        : Menu;

    private string EmptyStateIcon => IsSearching ? Icons.Material.Filled.SearchOff : Icons.Material.Filled.MenuBook;

    private string EmptyStateText => IsSearching ? "Nothing found" : "Nothing on the menu";

    private string EmptyStateDetail => IsSearching
        ? "Try another name."
        : "The admin hasn't put anything on the menu yet.";

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

    private void OpenCategory(MenuCategoryDto category)
    {
        NavigationManager.NavigateTo($"{TableCode.ToOrderPadUri()}/{category.Id}");
    }
}
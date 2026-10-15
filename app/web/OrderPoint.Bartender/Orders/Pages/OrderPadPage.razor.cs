using Microsoft.AspNetCore.Components;
using OrderPoint.Bartender.Orders.Services;
using OrderPoint.Bartender.Shared.Extensions;
using OrderPoint.Bartender.Shared.Layout;
using OrderPoint.Shared.Menu.Api;
using OrderPoint.Shared.Menu.Dtos;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Services;

namespace OrderPoint.Bartender.Orders.Pages;

// Categories first, then a category's items; the order stays a draft while the bartender looks around
public sealed partial class OrderPadPage : IDisposable
{
    // Above the bottom navigation while the list scrolls under it
    private const string SummaryBarStyle =
        $"position: sticky; bottom: calc({BottomNavigation.Height} + 12px); z-index: 1;";

    private const int LoadingCardsCount = 6;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private MenuApiClient MenuApiClient { get; set; } = null!;

    [Inject]
    private OrderDraftService OrderDraftService { get; set; } = null!;

    [Inject]
    private OrdersHubService OrdersHubService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    // Null at the counter
    [Parameter]
    public string? TableCode { get; set; }

    private IReadOnlyList<MenuCategoryDto> Menu { get; set; } = [];

    private bool IsLoading { get; set; } = true;

    private Guid? SelectedCategoryId { get; set; }

    private OrderDraft Draft => OrderDraftService.Get(TableCode);

    private MenuCategoryDto? SelectedCategory => Menu.FirstOrDefault(category => category.Id == SelectedCategoryId);

    private string PlaceText => TableCode.ToPlaceText();

    private string SubtitleText => SelectedCategory?.Name ?? "New order";

    private string BackLabel => SelectedCategory is null ? "Back to the tables" : "Back to the categories";

    // Today's prices; a line whose item left the menu counts nothing
    private decimal Total => Draft.Lines.Sum(line => (FindItem(line.ItemId)?.Price ?? 0) * line.Quantity);

    protected override async Task OnInitializedAsync()
    {
        OrdersHubService.MenuChanged += OnMenuChanged;

        await GetMenuAsync();
    }

    public void Dispose()
    {
        OrdersHubService.MenuChanged -= OnMenuChanged;
    }

    private void OnMenuChanged()
    {
        _ = InvokeAsync(() => GetMenuAsync(isLiveUpdate: true));
    }

    // A live update shows no skeleton, and a failed one keeps what is shown
    private async Task GetMenuAsync(bool isLiveUpdate = false)
    {
        IReadOnlyList<MenuCategoryDto>? menu = await ApiService.ExecuteAsync(() => MenuApiClient.GetMenuAsync());

        if (menu is not null || !isLiveUpdate)
        {
            Menu = menu ?? [];
        }

        IsLoading = false;

        StateHasChanged();
    }

    private MenuItemDto? FindItem(Guid itemId)
    {
        return Menu.SelectMany(category => category.Items).FirstOrDefault(item => item.Id == itemId);
    }

    private void SelectCategory(MenuCategoryDto category)
    {
        SelectedCategoryId = category.Id;
    }

    // Tapping a row adds one too: quicker than aiming for the button
    private void AddItem(MenuItemDto item)
    {
        Draft.SetQuantity(item.Id, item.Name, Draft.GetQuantity(item.Id) + 1);
    }

    private void SetQuantity(MenuItemDto item, int quantity)
    {
        Draft.SetQuantity(item.Id, item.Name, quantity);
    }

    private void GoBack()
    {
        if (SelectedCategoryId is not null)
        {
            SelectedCategoryId = null;

            return;
        }

        NavigationManager.NavigateTo("/");
    }

    private void Review()
    {
        NavigationManager.NavigateTo($"{TableCode.ToOrderPadUri()}/review");
    }
}
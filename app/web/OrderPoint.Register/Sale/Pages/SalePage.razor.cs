using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Register.Menu.Services;
using OrderPoint.Register.Orders.Api;
using OrderPoint.Register.Orders.Api.Requests;
using OrderPoint.Register.Orders.Services;
using OrderPoint.Register.Sale.Dtos;
using OrderPoint.Register.Sale.Services;
using OrderPoint.Shared.Errors;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Menu.Dtos;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;
using OrderPoint.Shared.Services;

namespace OrderPoint.Register.Sale.Pages;

// The till: a category, then its items onto the sale below, then cash or card
public sealed partial class SalePage : IDisposable
{
    // Two rows on a desktop
    private const int LoadingCardsCount = 8;

    // As tall as the screen under the app bar (64px) and the page padding, so the sale sits at the bottom
    private const string PageStyle = "display: flex; flex-direction: column; min-height: calc(100vh - 88px);";

    private const string ItemIsUnavailableCode = "Order.ItemIsUnavailable";

    private const string ItemNotFoundCode = "Item.NotFound";

    [Inject]
    private MenuService MenuService { get; set; } = null!;

    [Inject]
    private SaleService SaleService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private OrderApiClient OrderApiClient { get; set; } = null!;

    [Inject]
    private PaymentDialogService PaymentDialogService { get; set; } = null!;

    private IReadOnlyList<MenuCategoryDto> Menu { get; set; } = [];

    private bool IsLoading { get; set; } = true;

    private bool IsPaying { get; set; }

    private Guid? SelectedCategoryId { get; set; }

    private string? SearchQuery { get; set; }

    private bool IsSearching => !string.IsNullOrWhiteSpace(SearchQuery);

    // Null while the categories show, or once the category left the menu
    private MenuCategoryDto? SelectedCategory => Menu.FirstOrDefault(category => category.Id == SelectedCategoryId);

    private IEnumerable<MenuCategoryDto> FoundCategories => IsSearching
        ? Menu.Where(category =>
            category.Name.Contains(SearchQuery!.Trim(), StringComparison.CurrentCultureIgnoreCase))
        : Menu;

    private IEnumerable<MenuItemDto> FoundItems => IsSearching
        ? (SelectedCategory?.Items ?? [])
            .Where(item => item.Name.Contains(SearchQuery!.Trim(), StringComparison.CurrentCultureIgnoreCase))
        : SelectedCategory?.Items ?? [];

    private bool HasCards => SelectedCategory is null ? FoundCategories.Any() : FoundItems.Any();

    private string SubtitleText => SelectedCategory is null
        ? "Pick a category, or search for one."
        : string.IsNullOrWhiteSpace(SelectedCategory.Description)
            ? SelectedCategory.Items.Count.ToCountText("item", "items")
            : SelectedCategory.Description;

    private IReadOnlyList<(SaleLineDto Line, MenuItemDto? Item)> Rows
    {
        get
        {
            Dictionary<Guid, MenuItemDto> items = Menu
                .SelectMany(category => category.Items)
                .ToDictionary(item => item.Id);

            return SaleService.Lines.Select(line => (line, items.GetValueOrDefault(line.ItemId))).ToList();
        }
    }

    private int ItemsCount => SaleService.Lines.Sum(line => line.Quantity);

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

    // New prices, and lines whose item just left the menu; a failed reload keeps what is shown
    private void OnMenuChanged()
    {
        _ = InvokeAsync(GetMenuAsync);
    }

    private async Task GetMenuAsync()
    {
        IReadOnlyList<MenuCategoryDto>? menu = await MenuService.GetMenuAsync();

        if (menu is null)
        {
            return;
        }

        Menu = menu;

        StateHasChanged();
    }

    private void SelectCategory(MenuCategoryDto category)
    {
        SelectedCategoryId = category.Id;
        SearchQuery = null;
    }

    private void ShowCategories()
    {
        SelectedCategoryId = null;
        SearchQuery = null;
    }

    private void AddItem(MenuItemDto item)
    {
        SaleService.SetQuantity(item.Id, item.Name, SaleService.GetQuantity(item.Id) + 1);
    }

    private void SetQuantity((SaleLineDto Line, int Quantity) change)
    {
        SaleService.SetQuantity(change.Line.ItemId, change.Line.Name, change.Quantity);
    }

    private async Task PayInCashAsync()
    {
        decimal? cashReceived = await PaymentDialogService.AskForCashAsync(SaleService.GetTotal(Menu));

        if (cashReceived is null)
        {
            return;
        }

        await SellAsync(new PayOrderRequest(PaymentMethod.Cash, cashReceived));
    }

    private async Task PayByCardAsync()
    {
        bool isApproved = await PaymentDialogService.ConfirmCardAsync(SaleService.GetTotal(Menu));

        if (!isApproved)
        {
            return;
        }

        await SellAsync(new PayOrderRequest(PaymentMethod.Card, CashReceived: null));
    }

    // The receipt says it was paid, so no snackbar
    private async Task SellAsync(PayOrderRequest payment)
    {
        IsPaying = true;

        OrderDto? order = await ApiService.ExecuteAsync(() => CreateSaleAsync(payment));

        IsPaying = false;

        if (order is null)
        {
            return;
        }

        SaleService.Clear();

        await PaymentDialogService.ShowReceiptAsync(order);
    }

    private async Task<OrderDto> CreateSaleAsync(PayOrderRequest payment)
    {
        CreateRegisterOrderRequest request = new(
            string.IsNullOrWhiteSpace(SaleService.CustomerName) ? null : SaleService.CustomerName,
            string.IsNullOrWhiteSpace(SaleService.Note) ? null : SaleService.Note,
            SaleService.Lines.Select(line => new CreateOrderItemRequest(line.ItemId, line.Quantity)).ToList(),
            payment);

        try
        {
            return await OrderApiClient.CreateRegisterOrderAsync(request);
        }
        catch (ApiException exception)
            when (exception.HasErrorCode(ItemIsUnavailableCode) || exception.HasErrorCode(ItemNotFoundCode))
        {
            // The menu changed since it was loaded: reload it, so the gone lines are marked
            MenuService.Reset();

            await GetMenuAsync();

            throw;
        }
    }
}
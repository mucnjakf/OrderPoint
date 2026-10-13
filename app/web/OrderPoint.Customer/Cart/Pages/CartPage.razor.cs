using Microsoft.AspNetCore.Components;
using OrderPoint.Customer.Cart.Dtos;
using OrderPoint.Customer.Cart.Services;
using OrderPoint.Customer.Menu.Dtos;
using OrderPoint.Customer.Menu.Services;
using OrderPoint.Customer.Orders.Api;
using OrderPoint.Customer.Orders.Api.Requests;
using OrderPoint.Customer.Tables.Services;
using OrderPoint.Shared.Errors;
using OrderPoint.Shared.Services;

namespace OrderPoint.Customer.Cart.Pages;

// Prices and pictures come from the current menu, so the total is what the bar charges
public sealed partial class CartPage : IDisposable
{
    // As long as the API accepts
    private const int NoteMaxLength = 200;

    private const int LoadingRowsCount = 3;

    private const string ItemIsUnavailableCode = "Order.ItemIsUnavailable";

    private const string ItemNotFoundCode = "Item.NotFound";

    [Inject]
    private CartService CartService { get; set; } = null!;

    [Inject]
    private MenuService MenuService { get; set; } = null!;

    [Inject]
    private TableService TableService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private OrderApiClient OrderApiClient { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private Dictionary<Guid, MenuItemDto> MenuItems { get; set; } = [];

    private bool IsLoading { get; set; } = true;

    private bool IsPlacingOrder { get; set; }

    private string? Note { get; set; }

    private IEnumerable<(CartLineDto Line, MenuItemDto? Item)> Rows => CartService.Lines
        .Select(line => (line, MenuItems.GetValueOrDefault(line.ItemId)));

    private bool HasUnavailableLines => Rows.Any(row => row.Item is null);

    private decimal Total => Rows
        .Where(row => row.Item is not null)
        .Sum(row => row.Item!.Price * row.Line.Quantity);

    private bool CanPlaceOrder =>
        TableService.TableCode is not null && !HasUnavailableLines && !IsPlacingOrder && CartService.Lines.Count != 0;

    protected override async Task OnInitializedAsync()
    {
        CartService.Changed += OnCartChanged;

        Note = CartService.Note;

        await GetMenuItemsAsync();

        IsLoading = false;
    }

    public void Dispose()
    {
        CartService.Changed -= OnCartChanged;
    }

    private async Task GetMenuItemsAsync()
    {
        IReadOnlyList<MenuCategoryDto> menu = await MenuService.GetMenuAsync() ?? [];

        MenuItems = menu.SelectMany(category => category.Items).ToDictionary(item => item.Id);
    }

    private async Task OnNoteChangedAsync()
    {
        await CartService.SetNoteAsync(Note);
    }

    private async Task PlaceOrderAsync()
    {
        IsPlacingOrder = true;

        bool isSuccess = await ApiService.ExecuteAsync(CreateOrderAsync, "Your order was sent to the bar");

        IsPlacingOrder = false;

        if (!isSuccess)
        {
            return;
        }

        await CartService.ClearAsync();

        NavigationManager.NavigateTo("/orders");
    }

    private async Task CreateOrderAsync()
    {
        CreateOrderRequest request = new(
            TableService.TableCode!,
            CartService.Note,
            CartService.Lines.Select(line => new CreateOrderItemRequest(line.ItemId, line.Quantity)).ToList());

        try
        {
            await OrderApiClient.CreateOrderAsync(request);
        }
        catch (ApiException exception)
            when (exception.HasErrorCode(ItemIsUnavailableCode) || exception.HasErrorCode(ItemNotFoundCode))
        {
            // The menu changed since it was loaded: reload it, so the cart marks what is gone
            MenuService.Reset();

            await GetMenuItemsAsync();

            throw;
        }
    }

    private void OnCartChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }
}
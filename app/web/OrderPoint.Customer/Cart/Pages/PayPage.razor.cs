using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Customer.Cart.Dtos;
using OrderPoint.Customer.Cart.Services;
using OrderPoint.Customer.Menu.Dtos;
using OrderPoint.Customer.Menu.Services;
using OrderPoint.Customer.Orders.Api;
using OrderPoint.Customer.Orders.Api.Requests;
using OrderPoint.Customer.Orders.Services;
using OrderPoint.Customer.Tables.Services;
using OrderPoint.Shared.Errors;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Services;

namespace OrderPoint.Customer.Cart.Pages;

// Guests pay when ordering: the card is charged, then the order goes to the bar
public sealed partial class PayPage : IDisposable
{
    private const string CartUri = "/cart";

    private const string ItemIsUnavailableCode = "Order.ItemIsUnavailable";

    private const string ItemNotFoundCode = "Item.NotFound";

    private const string TableNotFoundCode = "Table.NotFound";

    private const string TableIsInactiveCode = "Table.IsInactive";

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

    [Inject]
    private MyOrdersService MyOrdersService { get; set; } = null!;

    [Inject]
    private OrderTrackingHubService OrderTrackingHubService { get; set; } = null!;

    private CreateOrderCardRequest Card { get; } = new();

    // Digits only, with the spaces and the slash typed in for the guest; a mask keeps state, so one per page
    private PatternMask CardNumberMask { get; } = new("0000 0000 0000 0000 000");

    private PatternMask ExpiryMask { get; } = new("00/00");

    private PatternMask CvcMask { get; } = new("0000");

    private Dictionary<Guid, MenuItemDto> MenuItems { get; set; } = [];

    private bool IsLoading { get; set; } = true;

    private bool IsPaying { get; set; }

    private IEnumerable<(CartLineDto Line, MenuItemDto? Item)> Rows => CartService.Lines
        .Select(line => (line, MenuItems.GetValueOrDefault(line.ItemId)));

    private bool HasUnavailableLines => Rows.Any(row => row.Item is null);

    private decimal Total => Rows
        .Where(row => row.Item is not null)
        .Sum(row => row.Item!.Price * row.Line.Quantity);

    private bool CanPay =>
        TableService.TableCode is not null && !HasUnavailableLines && !IsPaying && CartService.Lines.Count != 0;

    private string SummaryText =>
        $"Table {TableService.TableCode} · {CartService.ItemsCount.ToCountText("item", "items")}";

    protected override async Task OnInitializedAsync()
    {
        MenuService.Changed += OnMenuChanged;

        await GetMenuItemsAsync();

        IsLoading = false;
    }

    public void Dispose()
    {
        MenuService.Changed -= OnMenuChanged;
    }

    private async Task GetMenuItemsAsync()
    {
        IReadOnlyList<MenuCategoryDto> menu = await MenuService.GetMenuAsync() ?? [];

        MenuItems = menu.SelectMany(category => category.Items).ToDictionary(item => item.Id);
    }

    // New prices, and lines whose item just left the menu; a failed reload keeps what is shown
    private void OnMenuChanged()
    {
        _ = InvokeAsync(async () =>
        {
            IReadOnlyList<MenuCategoryDto>? menu = await MenuService.GetMenuAsync();

            if (menu is null)
            {
                return;
            }

            MenuItems = menu.SelectMany(category => category.Items).ToDictionary(item => item.Id);

            StateHasChanged();
        });
    }

    private async Task PayAsync()
    {
        if (!CanPay)
        {
            return;
        }

        IsPaying = true;

        OrderDto? order = await ApiService.ExecuteAsync(CreateOrderAsync);

        IsPaying = false;

        if (order is null)
        {
            return;
        }

        await MyOrdersService.AddAsync(order.Id, order.CreatedAtUtc);
        await OrderTrackingHubService.TrackAsync(order.Id);
        await CartService.ClearAsync();

        // The status page says it was sent and paid, so no snackbar
        NavigationManager.NavigateTo($"/orders/{order.Id}");
    }

    private async Task<OrderDto> CreateOrderAsync()
    {
        CreateOrderRequest request = new(
            TableService.TableCode!,
            CartService.Note,
            CartService.Lines.Select(line => new CreateOrderItemRequest(line.ItemId, line.Quantity)).ToList(),
            Card);

        try
        {
            return await OrderApiClient.CreateOrderAsync(request);
        }
        catch (ApiException exception)
            when (exception.HasErrorCode(ItemIsUnavailableCode) || exception.HasErrorCode(ItemNotFoundCode))
        {
            // The menu changed since it was loaded: reload it, so the summary marks what is gone
            MenuService.Reset();

            await GetMenuItemsAsync();

            throw;
        }
        catch (ApiException exception)
            when (exception.HasErrorCode(TableNotFoundCode) || exception.HasErrorCode(TableIsInactiveCode))
        {
            // The table was removed or closed since it was scanned: the cart asks for a scan again
            await TableService.ClearAsync();

            NavigationManager.NavigateTo(CartUri);

            throw;
        }
    }
}
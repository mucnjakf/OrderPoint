using Microsoft.AspNetCore.Components;
using OrderPoint.Customer.Cart.Dtos;
using OrderPoint.Customer.Cart.Services;
using OrderPoint.Customer.Menu.Dtos;
using OrderPoint.Customer.Menu.Services;
using OrderPoint.Customer.Tables.Services;

namespace OrderPoint.Customer.Cart.Pages;

// Prices and pictures come from the current menu, so the total is what the bar charges
public sealed partial class CartPage : IDisposable
{
    // As long as the API accepts
    private const int NoteMaxLength = 200;

    private const int LoadingRowsCount = 3;

    [Inject]
    private CartService CartService { get; set; } = null!;

    [Inject]
    private MenuService MenuService { get; set; } = null!;

    [Inject]
    private TableService TableService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private Dictionary<Guid, MenuItemDto> MenuItems { get; set; } = [];

    private bool IsLoading { get; set; } = true;

    private string? Note { get; set; }

    private IEnumerable<(CartLineDto Line, MenuItemDto? Item)> Rows => CartService.Lines
        .Select(line => (line, MenuItems.GetValueOrDefault(line.ItemId)));

    private bool HasUnavailableLines => Rows.Any(row => row.Item is null);

    private decimal Total => Rows
        .Where(row => row.Item is not null)
        .Sum(row => row.Item!.Price * row.Line.Quantity);

    private bool CanCheckOut =>
        TableService.TableCode is not null && !HasUnavailableLines && CartService.Lines.Count != 0;

    protected override async Task OnInitializedAsync()
    {
        CartService.Changed += OnCartChanged;
        MenuService.Changed += OnMenuChanged;

        Note = CartService.Note;

        await GetMenuItemsAsync();

        IsLoading = false;
    }

    public void Dispose()
    {
        CartService.Changed -= OnCartChanged;
        MenuService.Changed -= OnMenuChanged;
    }

    private async Task GetMenuItemsAsync()
    {
        IReadOnlyList<MenuCategoryDto> menu = await MenuService.GetMenuAsync() ?? [];

        MenuItems = menu.SelectMany(category => category.Items).ToDictionary(item => item.Id);
    }

    // New prices and pictures, and lines whose item just left the menu; a failed reload keeps what is shown
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

    private async Task OnNoteChangedAsync()
    {
        await CartService.SetNoteAsync(Note);
    }

    private void CheckOut()
    {
        NavigationManager.NavigateTo("/cart/pay");
    }

    // Every line and the note at once
    private async Task ClearCartAsync()
    {
        Note = null;

        await CartService.ClearAsync();
    }

    private void OnCartChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }
}
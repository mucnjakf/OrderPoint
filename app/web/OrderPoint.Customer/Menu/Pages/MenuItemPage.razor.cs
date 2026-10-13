using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Customer.Cart.Services;
using OrderPoint.Customer.Menu.Dtos;
using OrderPoint.Customer.Menu.Services;
using OrderPoint.Customer.Shared.Extensions;

namespace OrderPoint.Customer.Menu.Pages;

// Under its category's route, so the back arrow returns to that category. Before the item is in the cart the page
// adds it; afterwards it changes the quantity there or removes it.
public sealed partial class MenuItemPage : IDisposable
{
    [Inject]
    private MenuService MenuService { get; set; } = null!;

    [Inject]
    private CartService CartService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public Guid CategoryId { get; set; }

    [Parameter]
    public Guid ItemId { get; set; }

    private MenuCategoryDto? Category { get; set; }

    private MenuItemDto? Item { get; set; }

    private bool IsLoading { get; set; } = true;

    private int Quantity { get; set; } = 1;

    private int CartQuantity => CartService.GetQuantity(ItemId);

    private string AddButtonText => $"Add · {(Item!.Price * Quantity).ToString("C", CultureInfo.CurrentCulture)}";

    protected override async Task OnInitializedAsync()
    {
        CartService.Changed += OnCartChanged;
        IReadOnlyList<MenuCategoryDto> menu = await MenuService.GetMenuAsync() ?? [];

        Category = menu.FirstOrDefault(category => category.Id == CategoryId);
        Item = Category?.Items.FirstOrDefault(item => item.Id == ItemId);

        IsLoading = false;
    }

    public void Dispose()
    {
        CartService.Changed -= OnCartChanged;
    }

    private async Task AddToCartAsync()
    {
        await CartService.AddAsync(Item!.Id, CategoryId, Item.Name, Quantity);

        Snackbar.AddCartConfirmation($"{Quantity}× {Item.Name} added", NavigationManager);

        Quantity = 1;
    }

    private async Task OnCartQuantityChangedAsync(int quantity)
    {
        await CartService.SetQuantityAsync(ItemId, quantity);
    }

    private void OnCartChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }
}
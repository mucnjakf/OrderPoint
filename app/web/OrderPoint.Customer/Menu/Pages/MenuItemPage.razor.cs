using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Customer.Cart.Services;
using OrderPoint.Customer.Menu.Dtos;
using OrderPoint.Customer.Menu.Services;
using OrderPoint.Customer.Shared.Extensions;

namespace OrderPoint.Customer.Menu.Pages;

// Under its category's route, so the back arrow returns to that category
public sealed partial class MenuItemPage
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

    // A line holds at most CartService.MaxQuantity
    private int MaxQuantityToAdd => CartService.MaxQuantity - CartQuantity;

    private string AddButtonText => $"Add · {(Item!.Price * Quantity).ToString("C", CultureInfo.CurrentCulture)}";

    protected override async Task OnInitializedAsync()
    {
        IReadOnlyList<MenuCategoryDto> menu = await MenuService.GetMenuAsync() ?? [];

        Category = menu.FirstOrDefault(category => category.Id == CategoryId);
        Item = Category?.Items.FirstOrDefault(item => item.Id == ItemId);

        IsLoading = false;
    }

    private async Task AddToCartAsync()
    {
        int quantity = Math.Min(Quantity, MaxQuantityToAdd);

        await CartService.AddAsync(Item!.Id, CategoryId, Item.Name, quantity);

        Snackbar.AddCartConfirmation($"{quantity}× {Item.Name} added", NavigationManager);

        Quantity = 1;
    }
}
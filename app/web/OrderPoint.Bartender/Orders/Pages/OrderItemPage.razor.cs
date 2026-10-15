using System.Globalization;
using Microsoft.AspNetCore.Components;
using OrderPoint.Bartender.Menu.Services;
using OrderPoint.Bartender.Orders.Services;
using OrderPoint.Bartender.Shared.Extensions;
using OrderPoint.Shared.Menu.Dtos;

namespace OrderPoint.Bartender.Orders.Pages;

// Under its category's route, so the back arrow returns to that category. Before the item is in the order the
// page adds it; afterwards it changes the quantity there or removes it.
public sealed partial class OrderItemPage : IDisposable
{
    [Inject]
    private MenuService MenuService { get; set; } = null!;

    [Inject]
    private OrderDraftService OrderDraftService { get; set; } = null!;

    // Null at the counter
    [Parameter]
    public string? TableCode { get; set; }

    [Parameter]
    public Guid CategoryId { get; set; }

    [Parameter]
    public Guid ItemId { get; set; }

    private MenuCategoryDto? Category { get; set; }

    private MenuItemDto? Item { get; set; }

    private bool IsLoading { get; set; } = true;

    private int Quantity { get; set; } = 1;

    private OrderDraft Draft => OrderDraftService.Get(TableCode);

    private int OrderQuantity => Draft.GetQuantity(ItemId);

    private string PlaceText => TableCode.ToPlaceText();

    private string CategoryUri => $"{TableCode.ToOrderPadUri()}/{CategoryId}";

    private string ReviewUri => $"{TableCode.ToOrderPadUri()}/review";

    private string AddButtonText => $"Add · {(Item!.Price * Quantity).ToString("C", CultureInfo.CurrentCulture)}";

    protected override async Task OnInitializedAsync()
    {
        MenuService.Changed += OnMenuChanged;

        IReadOnlyList<MenuCategoryDto> menu = await MenuService.GetMenuAsync() ?? [];

        ShowItemFrom(menu);

        IsLoading = false;
    }

    public void Dispose()
    {
        MenuService.Changed -= OnMenuChanged;
    }

    private void ShowItemFrom(IReadOnlyList<MenuCategoryDto> menu)
    {
        Category = menu.FirstOrDefault(category => category.Id == CategoryId);
        Item = Category?.Items.FirstOrDefault(item => item.Id == ItemId);
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

            ShowItemFrom(menu);

            StateHasChanged();
        });
    }

    private void AddToOrder()
    {
        Draft.SetQuantity(Item!.Id, Item.Name, Quantity);

        Quantity = 1;
    }

    private void SetOrderQuantity(int quantity)
    {
        Draft.SetQuantity(Item!.Id, Item.Name, quantity);
    }
}
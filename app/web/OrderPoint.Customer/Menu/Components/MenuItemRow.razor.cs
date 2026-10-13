using System.Globalization;
using Microsoft.AspNetCore.Components;
using OrderPoint.Customer.Cart.Services;
using OrderPoint.Customer.Menu.Dtos;

namespace OrderPoint.Customer.Menu.Components;

public sealed partial class MenuItemRow
{
    private const string SingleLineStyle =
        "min-width: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;";

    [Parameter]
    [EditorRequired]
    public MenuItemDto Item { get; set; } = null!;

    // How many are in the cart, shown on the + button
    [Parameter]
    [EditorRequired]
    public int CartQuantity { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<MenuItemDto> OnClick { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<MenuItemDto> OnAdd { get; set; }

    private string DetailsText => string.IsNullOrWhiteSpace(Item.Description)
        ? $"Portion {Item.Portion.ToString(CultureInfo.CurrentCulture)}"
        : $"Portion {Item.Portion.ToString(CultureInfo.CurrentCulture)} · {Item.Description}";

    private async Task OnRowClickAsync()
    {
        await OnClick.InvokeAsync(Item);
    }

    private async Task OnAddClickAsync()
    {
        await OnAdd.InvokeAsync(Item);
    }
}
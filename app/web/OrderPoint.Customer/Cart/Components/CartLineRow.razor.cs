using System.Globalization;
using Microsoft.AspNetCore.Components;
using OrderPoint.Customer.Cart.Dtos;
using OrderPoint.Customer.Cart.Services;
using OrderPoint.Shared.Constants;
using OrderPoint.Shared.Menu.Dtos;

namespace OrderPoint.Customer.Cart.Components;

public sealed partial class CartLineRow
{
    private const string ImageBaseStyle = $"{ImageStyles.Rounded} flex-shrink: 0;";

    [Parameter]
    [EditorRequired]
    public CartLineDto Line { get; set; } = null!;

    // The item as the menu has it now; null once it left the menu
    [Parameter]
    [EditorRequired]
    public MenuItemDto? Item { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<int> OnQuantityChanged { get; set; }

    private string ImageStyle => Item is null
        ? $"{ImageBaseStyle} filter: grayscale(1); opacity: 0.6;"
        : ImageBaseStyle;

    private string PriceText
    {
        get
        {
            CultureInfo culture = CultureInfo.CurrentCulture;

            string unitPriceText = Item!.Price.ToString("C", culture);
            string lineTotalText = (Item.Price * Line.Quantity).ToString("C", culture);

            return $"{unitPriceText} each · {lineTotalText}";
        }
    }

    private async Task OnQuantityChangedAsync(int quantity)
    {
        await OnQuantityChanged.InvokeAsync(quantity);
    }
}
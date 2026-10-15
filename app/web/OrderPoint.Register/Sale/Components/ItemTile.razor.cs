using Microsoft.AspNetCore.Components;
using OrderPoint.Shared.Menu.Dtos;

namespace OrderPoint.Register.Sale.Components;

// An item to tap onto the sale; the chip shows how many are on it already
public sealed partial class ItemTile
{
    private const string BaseStyle = "position: relative; overflow: hidden; height: 100%;";

    [Parameter]
    [EditorRequired]
    public MenuItemDto Item { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public int Quantity { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<MenuItemDto> OnClick { get; set; }

    private string TileStyle => Quantity == 0
        ? BaseStyle
        : $"{BaseStyle} border-color: var(--mud-palette-primary);";

    private async Task OnTileClickAsync()
    {
        await OnClick.InvokeAsync(Item);
    }
}
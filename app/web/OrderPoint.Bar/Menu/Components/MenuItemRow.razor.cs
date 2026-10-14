using System.Globalization;
using Microsoft.AspNetCore.Components;
using OrderPoint.Bar.Menu.Dtos;
using OrderPoint.Shared.Constants;

namespace OrderPoint.Bar.Menu.Components;

// One line of the menu; unavailable items stay listed, greyed, so a bartender can tell a guest they're out of it
public sealed partial class MenuItemRow
{
    private const string SingleLineStyle =
        "min-width: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;";

    private const string ImageRadiusStyle = $"{ImageStyles.Rounded} flex-shrink: 0;";

    [Parameter]
    [EditorRequired]
    public ItemDto Item { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public EventCallback<ItemDto> OnClick { get; set; }

    private string ImageStyle => Item.IsActive
        ? ImageRadiusStyle
        : $"{ImageRadiusStyle} filter: grayscale(1); opacity: 0.6;";

    private string DetailsText => string.IsNullOrWhiteSpace(Item.Description)
        ? $"Portion {Item.Portion.ToString(CultureInfo.CurrentCulture)}"
        : $"Portion {Item.Portion.ToString(CultureInfo.CurrentCulture)} · {Item.Description}";

    private async Task OnRowClickAsync()
    {
        await OnClick.InvokeAsync(Item);
    }
}
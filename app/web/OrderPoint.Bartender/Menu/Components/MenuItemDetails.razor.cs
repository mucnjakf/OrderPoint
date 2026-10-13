using Microsoft.AspNetCore.Components;
using OrderPoint.Bartender.Menu.Dtos;
using OrderPoint.Bartender.Menu.Enumerations;

namespace OrderPoint.Bartender.Menu.Components;

public sealed partial class MenuItemDetails
{
    private const string ImageBaseStyle =
        "display: block; width: 100%; height: 220px; border-radius: var(--mud-default-borderradius);";

    [Parameter]
    [EditorRequired]
    public ItemDto Item { get; set; } = null!;

    private string ImageStyle => Item.IsActive
        ? ImageBaseStyle
        : $"{ImageBaseStyle} filter: grayscale(1); opacity: 0.6;";

    private string? UnavailableText
    {
        get
        {
            if (Item.Status == ItemStatus.Inactive)
            {
                return "This item is inactive, so guests can't order it.";
            }

            if (Item.Category.Status == CategoryStatus.Inactive)
            {
                return $"The {Item.Category.Name} category is inactive, so guests can't order this item.";
            }

            return null;
        }
    }
}
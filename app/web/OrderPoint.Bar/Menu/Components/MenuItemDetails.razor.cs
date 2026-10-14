using Microsoft.AspNetCore.Components;
using OrderPoint.Bar.Menu.Dtos;
using OrderPoint.Bar.Menu.Enumerations;

namespace OrderPoint.Bar.Menu.Components;

// The Admin's item card, plus why an unavailable item can't be ordered
public sealed partial class MenuItemDetails
{
    [Parameter]
    [EditorRequired]
    public ItemDto Item { get; set; } = null!;

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
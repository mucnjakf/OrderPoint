using System.Globalization;
using Microsoft.AspNetCore.Components;
using OrderPoint.Bartender.Menu.Dtos;

namespace OrderPoint.Bartender.Menu.Components;

// One line of the menu; unavailable items stay listed, greyed, so a bartender can tell a guest they're out of it
public sealed partial class MenuItemRow
{
    private const string SingleLineStyle =
        "min-width: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;";

    private const string ImageRadiusStyle = "border-radius: var(--mud-default-borderradius); flex-shrink: 0;";

    [Parameter]
    [EditorRequired]
    public ItemDto Item { get; set; } = null!;

    // Under "All" the row says which category the item belongs to
    [Parameter]
    public bool ShowsCategory { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<ItemDto> OnClick { get; set; }

    private string ImageStyle => Item.IsActive
        ? ImageRadiusStyle
        : $"{ImageRadiusStyle} filter: grayscale(1); opacity: 0.6;";

    private string DetailsText
    {
        get
        {
            List<string> details = [];

            if (ShowsCategory)
            {
                details.Add(Item.Category.Name);
            }

            details.Add($"Portion {Item.Portion.ToString(CultureInfo.CurrentCulture)}");

            if (!string.IsNullOrWhiteSpace(Item.Description))
            {
                details.Add(Item.Description);
            }

            return string.Join(" · ", details);
        }
    }

    private async Task OnRowClickAsync()
    {
        await OnClick.InvokeAsync(Item);
    }
}
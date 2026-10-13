using System.Globalization;
using Microsoft.AspNetCore.Components;
using OrderPoint.Customer.Menu.Dtos;

namespace OrderPoint.Customer.Menu.Components;

public sealed partial class MenuItemRow
{
    private const string SingleLineStyle =
        "min-width: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;";

    [Parameter]
    [EditorRequired]
    public MenuItemDto Item { get; set; } = null!;

    // Search results come from every category, so they say which one
    [Parameter]
    public string? CategoryName { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<MenuItemDto> OnClick { get; set; }

    private string DetailsText
    {
        get
        {
            List<string> details = [];

            if (CategoryName is not null)
            {
                details.Add(CategoryName);
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
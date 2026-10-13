using Microsoft.AspNetCore.Components;
using OrderPoint.Bartender.Menu.Dtos;
using OrderPoint.Bartender.Menu.Enumerations;

namespace OrderPoint.Bartender.Menu.Components;

public sealed partial class MenuCategoryCard
{
    private const string ImageBaseStyle = "border-radius: var(--mud-default-borderradius);";

    // Three cards fit in a row on a phone, so a long name wraps onto a second line before it is cut off
    private const string NameStyle =
        "max-width: 100%; overflow-wrap: anywhere; display: -webkit-box; -webkit-line-clamp: 2; " +
        "-webkit-box-orient: vertical; overflow: hidden;";

    [Parameter]
    [EditorRequired]
    public CategoryDto Category { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public EventCallback<CategoryDto> OnClick { get; set; }

    private string ImageStyle => Category.Status is CategoryStatus.Active
        ? ImageBaseStyle
        : $"{ImageBaseStyle} filter: grayscale(1); opacity: 0.6;";

    private async Task OnCardClickAsync()
    {
        await OnClick.InvokeAsync(Category);
    }
}
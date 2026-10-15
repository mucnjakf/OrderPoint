using Microsoft.AspNetCore.Components;
using OrderPoint.Customer.Menu.Dtos;

namespace OrderPoint.Customer.Menu.Components;

public sealed partial class MenuCategoryCard
{
    // A long name wraps onto a second line before it is cut off
    private const string NameStyle =
        "max-width: 100%; overflow-wrap: anywhere; display: -webkit-box; -webkit-line-clamp: 2; " +
        "-webkit-box-orient: vertical; overflow: hidden;";

    [Parameter]
    [EditorRequired]
    public MenuCategoryDto Category { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public EventCallback<MenuCategoryDto> OnClick { get; set; }

    private async Task OnCardClickAsync()
    {
        await OnClick.InvokeAsync(Category);
    }
}
using Microsoft.AspNetCore.Components;
using OrderPoint.Bartender.Menu.Dtos;
using OrderPoint.Bartender.Menu.Enumerations;

namespace OrderPoint.Bartender.Menu.Components;

public sealed partial class MenuCategoryCard
{
    private const string ImageBaseStyle = "display: block; width: 100%; height: 120px;";

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
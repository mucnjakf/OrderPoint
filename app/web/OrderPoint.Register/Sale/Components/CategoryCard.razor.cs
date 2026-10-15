using Microsoft.AspNetCore.Components;
using OrderPoint.Shared.Menu.Dtos;

namespace OrderPoint.Register.Sale.Components;

// A category to sell from: picture, description, how many items and from what price
public sealed partial class CategoryCard
{
    [Parameter]
    [EditorRequired]
    public MenuCategoryDto Category { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public EventCallback<MenuCategoryDto> OnClick { get; set; }

    private decimal? LowestPrice => Category.Items.Count == 0 ? null : Category.Items.Min(item => item.Price);

    private async Task OnCardClickAsync()
    {
        await OnClick.InvokeAsync(Category);
    }
}
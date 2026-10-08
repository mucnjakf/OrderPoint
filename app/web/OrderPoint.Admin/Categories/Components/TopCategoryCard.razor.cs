using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Categories.Dtos;

namespace OrderPoint.Admin.Categories.Components;

public sealed partial class TopCategoryCard
{
    [Parameter]
    [EditorRequired]
    public int Rank { get; set; }

    [Parameter]
    [EditorRequired]
    public CategoryDto Category { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public EventCallback<CategoryDto> OnClick { get; set; }

    private async Task OnCardClickAsync()
    {
        await OnClick.InvokeAsync(Category);
    }
}
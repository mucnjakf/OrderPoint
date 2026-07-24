using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Categories.Dtos;

namespace OrderPoint.Admin.Categories.Pages.Details.Components;

public sealed partial class CategoryDetailsPaper
{
    [Parameter]
    [EditorRequired]
    public CategoryDto Category { get; set; }

    [Parameter]
    public EventCallback OnUpdateClick { get; set; }

    [Parameter]
    public EventCallback OnDeleteClick { get; set; }

    private async Task OnUpdateClickAsync()
    {
        await OnUpdateClick.InvokeAsync();
    }

    private async Task OnDeleteClickAsync()
    {
        await OnDeleteClick.InvokeAsync();
    }
}
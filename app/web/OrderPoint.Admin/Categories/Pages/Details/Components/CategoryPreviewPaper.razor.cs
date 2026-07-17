using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Categories.Dtos;

namespace OrderPoint.Admin.Categories.Pages.Details.Components;

public sealed partial class CategoryPreviewPaper
{
    [Parameter]
    [EditorRequired]
    public CategoryDto Category { get; set; }
}
using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Features.Categories.Dtos;

namespace OrderPoint.Admin.Features.Categories.Components;

public sealed partial class CategoriesTableRow
{
    [Parameter]
    [EditorRequired]
    public CategoryDto Category { get; set; } = null!;
}
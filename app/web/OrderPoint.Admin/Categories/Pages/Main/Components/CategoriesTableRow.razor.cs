using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Categories.Dtos;

namespace OrderPoint.Admin.Categories.Pages.Main.Components;

public sealed partial class CategoriesTableRow
{
    [Parameter]
    [EditorRequired]
    public CategoryDto Category { get; set; } = null!;
}
using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Features.Categories.Dtos;

namespace OrderPoint.Admin.Features.Categories.Components;

public sealed partial class TopCategoryPaper
{
    [Parameter]
    [EditorRequired]
    public CategoryDto Category { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public Guid? HoveredCategoryId { get; set; }

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;
}
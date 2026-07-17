using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Features.Categories.Dtos;

namespace OrderPoint.Admin.Features.Categories.Components;

public sealed partial class TopCategoryStack
{
    [Parameter]
    [EditorRequired]
    public IReadOnlyList<CategoryDto> Categories { get; set; } = null!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    private Guid? HoveredCategoryId { get; set; }
}
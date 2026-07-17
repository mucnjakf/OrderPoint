using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Categories.Dtos;

namespace OrderPoint.Admin.Categories.Pages.Main.Components;

public sealed partial class TopCategoryStack
{
    [Parameter]
    [EditorRequired]
    public IReadOnlyList<CategoryDto> Categories { get; set; } = [];

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    private Guid? HoveredCategoryId { get; set; }
}
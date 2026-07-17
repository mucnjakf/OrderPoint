using Microsoft.AspNetCore.Components;

namespace OrderPoint.Admin.Categories.Pages.Update.Components;

public sealed partial class UpdateCategoryDeletePaper
{
    [Parameter]
    [EditorRequired]
    public bool CategoryContainsItems { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback OnDeleteClick { get; set; }
}
using Microsoft.AspNetCore.Components;

namespace OrderPoint.Admin.Features.Categories.Dialogs;

public sealed partial class DeleteCategoryDialog
{
    [Parameter]
    public string CategoryName { get; set; } = null!;
}
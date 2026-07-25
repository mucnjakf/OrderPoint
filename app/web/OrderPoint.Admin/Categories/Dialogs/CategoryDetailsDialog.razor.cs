using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Categories.Dtos;

namespace OrderPoint.Admin.Categories.Dialogs;

public sealed partial class CategoryDetailsDialog
{
    [Parameter]
    public CategoryDto Category { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private void Close()
    {
        MudDialogInstance.Close(DialogResult.Ok(true));
    }
}
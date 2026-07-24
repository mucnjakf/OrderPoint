using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace OrderPoint.Admin.Categories.Dialogs;

public sealed partial class DeleteCategoryDialog
{
    [Parameter]
    public string CategoryName { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private void Confirm()
    {
        MudDialogInstance.Close(DialogResult.Ok(true));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}
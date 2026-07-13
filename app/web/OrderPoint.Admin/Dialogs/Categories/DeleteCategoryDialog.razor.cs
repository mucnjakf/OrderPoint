using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace OrderPoint.Admin.Dialogs.Categories;

public sealed partial class DeleteCategoryDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    [Parameter]
    public string CategoryName { get; set; } = null!;

    private void Confirm()
    {
        MudDialogInstance.Close(DialogResult.Ok(true));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}
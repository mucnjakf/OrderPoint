using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace OrderPoint.Admin.Items.Dialogs;

public sealed partial class DeleteItemDialog
{
    [Parameter]
    public string ItemName { get; set; } = null!;

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
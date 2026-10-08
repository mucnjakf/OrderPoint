using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace OrderPoint.Admin.Bartenders.Dialogs;

public sealed partial class DeleteBartenderDialog
{
    [Parameter]
    public string BartenderName { get; set; } = null!;

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
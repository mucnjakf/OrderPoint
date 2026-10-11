using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace OrderPoint.Admin.Account.Dialogs;

public sealed partial class SignOutDialog
{
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
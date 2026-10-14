using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace OrderPoint.Customer.Settings.Dialogs;

public sealed partial class ForgetOrdersDialog
{
    [Parameter]
    public int OrdersCount { get; set; }

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
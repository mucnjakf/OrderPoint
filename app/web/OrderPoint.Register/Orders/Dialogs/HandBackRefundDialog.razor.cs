using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace OrderPoint.Register.Orders.Dialogs;

public sealed partial class HandBackRefundDialog
{
    [Parameter]
    public int OrderNumber { get; set; }

    // What to do, e.g. "Hand €12.00 back to the guest in cash, then confirm."
    [Parameter]
    public string Text { get; set; } = null!;

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
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace OrderPoint.Customer.Orders.Dialogs;

public sealed partial class CancelOrderDialog
{
    [Parameter]
    public int OrderNumber { get; set; }

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
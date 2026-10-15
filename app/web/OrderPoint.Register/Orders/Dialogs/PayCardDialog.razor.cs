using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace OrderPoint.Register.Orders.Dialogs;

// The bar's own terminal charges the card; the register only records it
public sealed partial class PayCardDialog
{
    [Parameter]
    public decimal Total { get; set; }

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
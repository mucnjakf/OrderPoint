using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace OrderPoint.Customer.Cart.Dialogs;

public sealed partial class PlaceOrderDialog
{
    [Parameter]
    public string TableCode { get; set; } = null!;

    [Parameter]
    public int ItemsCount { get; set; }

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
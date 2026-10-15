using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Suppliers.Api.Requests;
using OrderPoint.Admin.Suppliers.Enumerations;

namespace OrderPoint.Admin.Suppliers.Dialogs;

public sealed partial class CreateSupplierDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private CreateSupplierRequest Request { get; set; } = new() { Status = SupplierStatus.Active };

    private void OnValidSubmit()
    {
        MudDialogInstance.Close(DialogResult.Ok(Request));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}
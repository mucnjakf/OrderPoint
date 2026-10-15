using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Suppliers.Api.Requests;
using OrderPoint.Admin.Suppliers.Dtos;

namespace OrderPoint.Admin.Suppliers.Dialogs;

public sealed partial class UpdateSupplierDialog
{
    [Parameter]
    public SupplierDto Supplier { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private UpdateSupplierRequest Request { get; set; } = null!;

    protected override void OnInitialized()
    {
        Request = new UpdateSupplierRequest
        {
            Name = Supplier.Name,
            ContactName = Supplier.ContactName,
            Phone = Supplier.Phone,
            Email = Supplier.Email,
            Note = Supplier.Note,
            Status = Supplier.Status
        };
    }

    private void OnValidSubmit()
    {
        MudDialogInstance.Close(DialogResult.Ok(Request));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}
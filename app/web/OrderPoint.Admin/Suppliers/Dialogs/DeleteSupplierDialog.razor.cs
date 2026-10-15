using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Suppliers.Dtos;
using OrderPoint.Shared.Extensions;

namespace OrderPoint.Admin.Suppliers.Dialogs;

public sealed partial class DeleteSupplierDialog
{
    [Parameter]
    public SupplierDto Supplier { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    // "Ana Kovac · 8 stock items"
    private string SummaryText =>
        $"{Supplier.ContactName ?? "No contact"} · {Supplier.StockItemsCount.ToCountText("stock item", "stock items")}";

    private void Confirm()
    {
        MudDialogInstance.Close(DialogResult.Ok(true));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}
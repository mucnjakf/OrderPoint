using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Shared.Tables.Dtos;

namespace OrderPoint.Admin.Tables.Dialogs;

public sealed partial class DeleteTableDialog
{
    [Parameter]
    public TableDto Table { get; set; } = null!;

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
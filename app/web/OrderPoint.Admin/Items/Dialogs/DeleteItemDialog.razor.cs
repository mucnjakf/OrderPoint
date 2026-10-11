using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Items.Dtos;

namespace OrderPoint.Admin.Items.Dialogs;

public sealed partial class DeleteItemDialog
{
    [Parameter]
    public ItemDto Item { get; set; } = null!;

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
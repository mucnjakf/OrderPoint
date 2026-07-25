using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Items.Dtos;

namespace OrderPoint.Admin.Items.Dialogs;

public sealed partial class ItemDetailsDialog
{
    [Parameter]
    public ItemDto Item { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private void Close()
    {
        MudDialogInstance.Close(DialogResult.Ok(true));
    }
}
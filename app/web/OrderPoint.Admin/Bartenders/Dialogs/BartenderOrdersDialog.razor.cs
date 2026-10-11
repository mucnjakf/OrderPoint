using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Bartenders.Dtos;

namespace OrderPoint.Admin.Bartenders.Dialogs;

public sealed partial class BartenderOrdersDialog
{
    [Parameter]
    public BartenderDto Bartender { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private void Close()
    {
        MudDialogInstance.Close(DialogResult.Ok(true));
    }
}
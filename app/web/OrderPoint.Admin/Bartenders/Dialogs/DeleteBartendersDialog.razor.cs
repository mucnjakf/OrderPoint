using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Bartenders.Dtos;

namespace OrderPoint.Admin.Bartenders.Dialogs;

public sealed partial class DeleteBartendersDialog
{
    [Parameter]
    public IReadOnlyList<BartenderDto> Bartenders { get; set; } = [];

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
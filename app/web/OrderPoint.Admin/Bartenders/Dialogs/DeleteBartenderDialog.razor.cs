using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Bartenders.Dtos;

namespace OrderPoint.Admin.Bartenders.Dialogs;

public sealed partial class DeleteBartenderDialog
{
    [Parameter]
    public BartenderDto Bartender { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private string BartenderName => $"{Bartender.FirstName} {Bartender.LastName}";

    private void Confirm()
    {
        MudDialogInstance.Close(DialogResult.Ok(true));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}
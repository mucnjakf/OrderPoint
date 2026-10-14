using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Tables.Api.Requests;
using OrderPoint.Shared.Tables.Dtos;
using OrderPoint.Shared.Tables.Enumerations;

namespace OrderPoint.Admin.Tables.Dialogs;

public sealed partial class CreateAreaDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private CreateAreaRequest Request { get; set; } = new();

    private void OnValidSubmit()
    {
        MudDialogInstance.Close(DialogResult.Ok(Request));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}
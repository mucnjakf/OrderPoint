using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Tables.Api.Requests;
using OrderPoint.Shared.Tables.Dtos;
using OrderPoint.Shared.Tables.Enumerations;

namespace OrderPoint.Admin.Tables.Dialogs;

public sealed partial class UpdateTableDialog
{
    [Parameter]
    public TableDto Table { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private UpdateTableRequest Request { get; set; } = new();

    protected override void OnInitialized()
    {
        Request = new UpdateTableRequest
        {
            Code = Table.Code,
            Seats = Table.Seats,
            Status = Table.Status,
            Shape = Table.Shape,
            Width = Table.Width,
            Height = Table.Height
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
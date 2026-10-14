using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Tables.Api.Requests;
using OrderPoint.Shared.Tables.Dtos;
using OrderPoint.Shared.Tables.Enumerations;

namespace OrderPoint.Admin.Tables.Dialogs;

public sealed partial class CreateTableDialog
{
    [Parameter]
    public string SuggestedCode { get; set; } = null!;

    [Parameter]
    public TableShape Shape { get; set; }

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private CreateTableRequest Request { get; set; } = new();

    protected override void OnInitialized()
    {
        Request = new CreateTableRequest
        {
            Code = SuggestedCode,
            Shape = Shape
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
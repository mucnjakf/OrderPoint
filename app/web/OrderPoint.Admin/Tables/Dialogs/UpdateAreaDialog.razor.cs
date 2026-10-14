using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Tables.Api.Requests;
using OrderPoint.Shared.Tables.Dtos;
using OrderPoint.Shared.Tables.Enumerations;

namespace OrderPoint.Admin.Tables.Dialogs;

public sealed partial class UpdateAreaDialog
{
    [Parameter]
    public AreaDto Area { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private UpdateAreaRequest Request { get; set; } = new();

    protected override void OnInitialized()
    {
        Request = new UpdateAreaRequest
        {
            Name = Area.Name,
            Columns = Area.Columns,
            Rows = Area.Rows
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
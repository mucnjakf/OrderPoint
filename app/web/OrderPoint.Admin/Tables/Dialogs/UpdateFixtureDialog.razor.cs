using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Tables.Api.Requests;
using OrderPoint.Shared.Tables.Dtos;
using OrderPoint.Shared.Tables.Enumerations;

namespace OrderPoint.Admin.Tables.Dialogs;

public sealed partial class UpdateFixtureDialog
{
    [Parameter]
    public FixtureDto Fixture { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private UpdateFixtureRequest Request { get; set; } = new();

    protected override void OnInitialized()
    {
        Request = new UpdateFixtureRequest
        {
            Type = Fixture.Type,
            Label = Fixture.Label,
            Width = Fixture.Width,
            Height = Fixture.Height
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
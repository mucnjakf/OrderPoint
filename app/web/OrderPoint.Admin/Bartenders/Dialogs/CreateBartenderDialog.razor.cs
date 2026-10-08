using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Bartenders.Api.Requests;
using OrderPoint.Admin.Shared.Dtos;
using OrderPoint.Admin.Shared.Extensions;

namespace OrderPoint.Admin.Bartenders.Dialogs;

public sealed partial class CreateBartenderDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private CreateBartenderRequest Request { get; set; } = new();

    private string? PreviewImageUrl => Request.Image?.ToDataUrl();

    private void OnImageSelected(ImageFileDto image)
    {
        Request.Image = image;
    }

    private void OnImageRemoved()
    {
        Request.Image = null;
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
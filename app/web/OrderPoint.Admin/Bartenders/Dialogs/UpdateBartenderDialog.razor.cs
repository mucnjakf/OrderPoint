using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Bartenders.Api.Requests;
using OrderPoint.Admin.Bartenders.Dtos;
using OrderPoint.Admin.Shared.Dtos;
using OrderPoint.Admin.Shared.Extensions;

namespace OrderPoint.Admin.Bartenders.Dialogs;

public sealed partial class UpdateBartenderDialog
{
    private const string CurrentImageFileName = "Current image";

    [Parameter]
    public BartenderDto Bartender { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private UpdateBartenderRequest Request { get; set; } = null!;

    protected override void OnInitialized()
    {
        Request = new UpdateBartenderRequest
        {
            FirstName = Bartender.FirstName,
            LastName = Bartender.LastName,
            Email = Bartender.Email,
            PhoneNumber = Bartender.PhoneNumber,
            Status = Bartender.Status,
            Notes = Bartender.Notes
        };
    }

    private string? PreviewImageUrl => Request.Image is not null
        ? Request.Image.ToDataUrl()
        : Request.RemoveImage ? null : Bartender.ImageUrl;

    private string? ImageFileName => Request.Image is not null
        ? Request.Image.FileName
        : PreviewImageUrl is null ? null : CurrentImageFileName;

    private void OnImageSelected(ImageFileDto image)
    {
        Request.Image = image;
        Request.RemoveImage = false;
    }

    private void OnImageRemoved()
    {
        Request.Image = null;
        Request.RemoveImage = true;
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
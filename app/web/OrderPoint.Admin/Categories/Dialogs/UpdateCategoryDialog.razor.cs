using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Categories.Api.Requests;
using OrderPoint.Admin.Categories.Dtos;
using OrderPoint.Admin.Shared.Dtos;
using OrderPoint.Admin.Shared.Extensions;

namespace OrderPoint.Admin.Categories.Dialogs;

public sealed partial class UpdateCategoryDialog
{
    [Parameter]
    public CategoryDto Category { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private UpdateCategoryRequest Request { get; set; } = null!;

    protected override void OnInitialized()
    {
        Request = new UpdateCategoryRequest
        {
            Name = Category.Name,
            Description = Category.Description,
            Status = Category.Status
        };
    }

    private string? PreviewImageUrl => Request.Image is not null
        ? Request.Image.ToDataUrl()
        : Request.RemoveImage ? null : Category.ImageUrl;

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
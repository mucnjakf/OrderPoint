using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using OrderPoint.Admin.Shared.Dtos;

namespace OrderPoint.Admin.Shared.Components;

public sealed partial class ImageUploadField
{
    private const long MaxImageSizeInBytes = 2 * 1024 * 1024;

    private const string AcceptedFileExtensions = ".jpg,.jpeg,.png,.webp";

    private const string HelperText = "JPEG, PNG or WebP, at most 2 MB.";

    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];

    [Parameter]
    [EditorRequired]
    public string? FileName { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<ImageFileDto> OnImageSelected { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback OnImageRemoved { get; set; }

    private string? ErrorText { get; set; }

    private async Task OnFileChangedAsync(IBrowserFile? file)
    {
        if (file is null)
        {
            return;
        }

        ErrorText = null;

        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            ErrorText = "Image must be a JPEG, PNG or WebP file.";
            return;
        }

        if (file.Size > MaxImageSizeInBytes)
        {
            ErrorText = "Image must be at most 2 MB.";
            return;
        }

        await using Stream stream = file.OpenReadStream(MaxImageSizeInBytes);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);

        await OnImageSelected.InvokeAsync(new ImageFileDto(memoryStream.ToArray(), file.ContentType, file.Name));
    }

    private async Task OnRemoveClickAsync()
    {
        await OnImageRemoved.InvokeAsync();
    }
}
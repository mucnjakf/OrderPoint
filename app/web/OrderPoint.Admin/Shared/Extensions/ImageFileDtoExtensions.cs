using OrderPoint.Admin.Shared.Dtos;

namespace OrderPoint.Admin.Shared.Extensions;

internal static class ImageFileDtoExtensions
{
    internal static string ToDataUrl(this ImageFileDto image)
    {
        return $"data:{image.ContentType};base64,{Convert.ToBase64String(image.Content)}";
    }
}
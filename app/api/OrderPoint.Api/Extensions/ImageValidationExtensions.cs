using FluentValidation;

namespace OrderPoint.Api.Extensions;

internal static class ImageValidationExtensions
{
    private const long MaxImageSizeInBytes = 2 * 1024 * 1024;

    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];

    internal static IRuleBuilderOptions<T, IFormFile> MustBeValidImage<T>(
        this IRuleBuilderInitial<T, IFormFile> ruleBuilder)
    {
        return ruleBuilder
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Image is required")
            .Must(image => image.Length <= MaxImageSizeInBytes).WithMessage("Image must be at most 2 MB")
            .Must(image => AllowedContentTypes.Contains(image.ContentType))
            .WithMessage("Image must be a JPEG, PNG or WebP file");
    }
}
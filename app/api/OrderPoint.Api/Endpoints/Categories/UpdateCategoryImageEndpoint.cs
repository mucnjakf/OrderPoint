using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Categories;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Categories;

internal sealed record UpdateCategoryImageRequest(IFormFile Image);

internal sealed class UpdateCategoryImageEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/categories/{id:guid}/image", HandleAsync)
            .WithName("UpdateCategoryImage")
            .WithTags("Categories")
            .DisableAntiforgery();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromForm] UpdateCategoryImageRequest request,
        [FromServices] IValidator<UpdateCategoryImageRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        await using Stream content = request.Image.OpenReadStream();

        UpdateCategoryImageCommand command = new(id, content, request.Image.ContentType);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class UpdateCategoryImageRequestValidator : AbstractValidator<UpdateCategoryImageRequest>
    {
        private const long MaxImageSizeInBytes = 2 * 1024 * 1024;

        private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];

        public UpdateCategoryImageRequestValidator()
        {
            RuleFor(request => request.Image)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Image is required")
                .Must(image => image.Length <= MaxImageSizeInBytes).WithMessage("Image must be at most 2 MB")
                .Must(image => AllowedContentTypes.Contains(image.ContentType))
                .WithMessage("Image must be a JPEG, PNG or WebP file");
        }
    }
}
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Items;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Items;

internal sealed record UpdateItemImageRequest(IFormFile Image);

internal sealed class UpdateItemImageEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/items/{id:guid}/image", HandleAsync)
            .WithName("UpdateItemImage")
            .WithTags("Items")
            .DisableAntiforgery();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromForm] UpdateItemImageRequest request,
        [FromServices] IValidator<UpdateItemImageRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        await using Stream content = request.Image.OpenReadStream();

        UpdateItemImageCommand command = new(id, content, request.Image.ContentType);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class UpdateItemImageRequestValidator : AbstractValidator<UpdateItemImageRequest>
    {
        private const long MaxImageSizeInBytes = 2 * 1024 * 1024;

        private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];

        public UpdateItemImageRequestValidator()
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
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Categories;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Categories;

/// <summary>The category image upload.</summary>
/// <param name="Image">The image file: JPEG, PNG or WebP, at most 2 MB.</param>
internal sealed record UpdateCategoryImageRequest(IFormFile Image);

internal sealed class UpdateCategoryImageEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/categories/{id:guid}/image", HandleAsync)
            .WithName("UpdateCategoryImage")
            .WithTags("Categories")
            .WithSummary("Upload a category image")
            .WithDescription("""
                Sets the category image. Send the file as `multipart/form-data` in the field `image`: JPEG, PNG or
                WebP, at most 2 MB.

                The previous image is deleted after the new one is saved.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .DisableAntiforgery()
            .RequireAuthorization(AuthorizationPolicies.Admin);
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
        public UpdateCategoryImageRequestValidator()
        {
            RuleFor(request => request.Image).MustBeValidImage();
        }
    }
}
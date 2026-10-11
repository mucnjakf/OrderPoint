using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Items;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Items;

/// <summary>The item image upload.</summary>
/// <param name="Image">The image file: JPEG, PNG or WebP, at most 2 MB.</param>
internal sealed record UpdateItemImageRequest(IFormFile Image);

internal sealed class UpdateItemImageEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/items/{id:guid}/image", HandleAsync)
            .WithName("UpdateItemImage")
            .WithTags("Items")
            .WithSummary("Upload an item image")
            .WithDescription("""
                Sets the item image. Send the file as `multipart/form-data` in the field `image`: JPEG, PNG or WebP,
                at most 2 MB.

                The previous image is deleted after the new one is saved.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .DisableAntiforgery()
            .RequireAuthorization(AuthorizationPolicies.Admin);
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
        public UpdateItemImageRequestValidator()
        {
            RuleFor(request => request.Image).MustBeValidImage();
        }
    }
}
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Bartenders;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Bartenders;

/// <summary>The bartender photo upload.</summary>
/// <param name="Image">The image file: JPEG, PNG or WebP, at most 2 MB.</param>
internal sealed record UpdateBartenderImageRequest(IFormFile Image);

internal sealed class UpdateBartenderImageEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/bartenders/{id:guid}/image", HandleAsync)
            .WithName("UpdateBartenderImage")
            .WithTags("Bartenders")
            .WithSummary("Upload a bartender image")
            .WithDescription("""
                Sets the bartender's photo. Send the file as `multipart/form-data` in the field `image`: JPEG, PNG or
                WebP, at most 2 MB.

                The previous image is deleted after the new one is saved.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .DisableAntiforgery()
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromForm] UpdateBartenderImageRequest request,
        [FromServices] IValidator<UpdateBartenderImageRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        await using Stream content = request.Image.OpenReadStream();

        UpdateBartenderImageCommand command = new(id, content, request.Image.ContentType);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class UpdateBartenderImageRequestValidator : AbstractValidator<UpdateBartenderImageRequest>
    {
        public UpdateBartenderImageRequestValidator()
        {
            RuleFor(request => request.Image).MustBeValidImage();
        }
    }
}
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Areas;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Areas;

/// <summary>The area's new values.</summary>
/// <param name="Name">The name, e.g. `Terrace`; unique.</param>
/// <param name="Columns">The width of the floor plan grid, in cells.</param>
/// <param name="Rows">The height of the floor plan grid, in cells.</param>
internal sealed record UpdateAreaRequest(string Name, int Columns, int Rows);

internal sealed class UpdateAreaEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/areas/{id:guid}", HandleAsync)
            .WithName("UpdateArea")
            .WithTags("Areas")
            .WithSummary("Update an area")
            .WithDescription("""
                Renames the area or resizes its grid.

                - `409 Area.NameAlreadyExists`: another area has this name
                - `409 Area.TooSmallForContents`: a table or fixture would lie outside the smaller grid
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateAreaRequest request,
        [FromServices] IValidator<UpdateAreaRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        UpdateAreaCommand command = new(id, request.Name, request.Columns, request.Rows);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class UpdateAreaRequestValidator : AbstractValidator<UpdateAreaRequest>
    {
        public UpdateAreaRequestValidator()
        {
            RuleFor(request => request.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(30).WithMessage("Name must be at most 30 characters");

            RuleFor(request => request.Columns)
                .InclusiveBetween(4, 48).WithMessage("Columns must be between 4 and 48");

            RuleFor(request => request.Rows)
                .InclusiveBetween(4, 48).WithMessage("Rows must be between 4 and 48");
        }
    }
}
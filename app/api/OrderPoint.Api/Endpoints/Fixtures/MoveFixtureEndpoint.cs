using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Fixtures;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Fixtures;

/// <summary>The fixture's new place on the floor plan.</summary>
/// <param name="AreaId">The id of the area, the same or another one.</param>
/// <param name="X">The left edge on the grid, in cells from 0.</param>
/// <param name="Y">The top edge on the grid, in cells from 0.</param>
/// <param name="Width">The width, in cells (swap with the height to rotate).</param>
/// <param name="Height">The height, in cells.</param>
internal sealed record MoveFixtureRequest(Guid AreaId, int X, int Y, int Width, int Height);

internal sealed class MoveFixtureEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/fixtures/{id:guid}/position", HandleAsync)
            .WithName("MoveFixture")
            .WithTags("Fixtures")
            .WithSummary("Move a fixture")
            .WithDescription("""
                Moves, resizes or rotates the fixture on the floor plan, also into another area. It must lie
                completely inside the area's grid.

                - `400 Fixture.OutsideArea`: the fixture would lie outside the area's grid
                - `404 Area.NotFound`: the area does not exist
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromBody] MoveFixtureRequest request,
        [FromServices] IValidator<MoveFixtureRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        MoveFixtureCommand command = new(id, request.AreaId, request.X, request.Y, request.Width, request.Height);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class MoveFixtureRequestValidator : AbstractValidator<MoveFixtureRequest>
    {
        public MoveFixtureRequestValidator()
        {
            RuleFor(request => request.AreaId)
                .NotEmpty().WithMessage("AreaId is required");

            RuleFor(request => request.X)
                .GreaterThanOrEqualTo(0).WithMessage("X must not be negative");

            RuleFor(request => request.Y)
                .GreaterThanOrEqualTo(0).WithMessage("Y must not be negative");

            RuleFor(request => request.Width)
                .InclusiveBetween(1, 48).WithMessage("Width must be between 1 and 48");

            RuleFor(request => request.Height)
                .InclusiveBetween(1, 48).WithMessage("Height must be between 1 and 48");
        }
    }
}
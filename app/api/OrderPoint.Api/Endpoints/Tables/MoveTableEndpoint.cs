using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Tables;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Tables;

/// <summary>The table's new place on the floor plan.</summary>
/// <param name="AreaId">The id of the area, the same or another one.</param>
/// <param name="X">The left edge on the grid, in cells from 0.</param>
/// <param name="Y">The top edge on the grid, in cells from 0.</param>
/// <param name="Width">The width, in cells (swap with the height to rotate).</param>
/// <param name="Height">The height, in cells.</param>
internal sealed record MoveTableRequest(Guid AreaId, int X, int Y, int Width, int Height);

internal sealed class MoveTableEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/tables/{id:guid}/position", HandleAsync)
            .WithName("MoveTable")
            .WithTags("Tables")
            .WithSummary("Move a table")
            .WithDescription("""
                Moves, resizes or rotates the table on the floor plan, also into another area; the floor plan editor
                calls it when a drag ends. It must lie completely inside the area's grid.

                - `400 Table.OutsideArea`: the table would lie outside the area's grid
                - `404 Area.NotFound`: the area does not exist
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromBody] MoveTableRequest request,
        [FromServices] IValidator<MoveTableRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        MoveTableCommand command = new(id, request.AreaId, request.X, request.Y, request.Width, request.Height);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class MoveTableRequestValidator : AbstractValidator<MoveTableRequest>
    {
        public MoveTableRequestValidator()
        {
            RuleFor(request => request.AreaId)
                .NotEmpty().WithMessage("AreaId is required");

            RuleFor(request => request.X)
                .GreaterThanOrEqualTo(0).WithMessage("X must not be negative");

            RuleFor(request => request.Y)
                .GreaterThanOrEqualTo(0).WithMessage("Y must not be negative");

            RuleFor(request => request.Width)
                .InclusiveBetween(1, 12).WithMessage("Width must be between 1 and 12");

            RuleFor(request => request.Height)
                .InclusiveBetween(1, 12).WithMessage("Height must be between 1 and 12");
        }
    }
}
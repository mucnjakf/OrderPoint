using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Areas;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Areas;

internal sealed class DeleteAreaEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapDelete("api/areas/{id:guid}", HandleAsync)
            .WithName("DeleteArea")
            .WithTags("Areas")
            .WithSummary("Delete an area")
            .WithDescription("""
                Deletes the area and its fixtures.

                - `409 Area.CannotDeleteAreaWithTables`: the area still has tables; move or delete them first.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        DeleteAreaCommand command = new(id);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }
}
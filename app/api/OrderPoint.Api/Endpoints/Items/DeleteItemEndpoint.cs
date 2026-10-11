using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Items;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Items;

internal sealed class DeleteItemEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapDelete("api/items/{id:guid}", HandleAsync)
            .WithName("DeleteItem")
            .WithTags("Items")
            .WithSummary("Delete an item")
            .WithDescription("""
                Deletes the item and its image.

                - `409 Item.CannotDeleteItemWithOrders`: the item is part of orders; deactivate it instead.
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
        DeleteItemCommand command = new(id);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }
}
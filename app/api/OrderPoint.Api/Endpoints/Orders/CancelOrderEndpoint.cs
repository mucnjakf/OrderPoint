using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Orders;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Orders;

internal sealed class CancelOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/orders/{id:guid}/cancel", HandleAsync)
            .WithName("CancelOrder")
            .WithTags("Orders")
            .WithSummary("Cancel an order")
            .WithDescription("""
                The guest who placed the order cancels it in the customer app, as long as no bartender has accepted
                it yet. No token is needed; the order id (returned when the order is placed) is not guessable.

                - `404 Order.NotFound`: the order does not exist
                - `409 Order.InvalidStatusTransition`: the order is not pending (e.g. a bartender accepted it first)
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        CancelOrderCommand command = new(id);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }
}
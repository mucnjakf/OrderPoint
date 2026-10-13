using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Orders;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Orders;

/// <summary>An order as the guest follows it.</summary>
/// <param name="Data">The order.</param>
internal sealed record GetOrderTrackingResponse(OrderTrackingDto Data);

internal sealed class GetOrderTrackingEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/orders/{id:guid}/tracking", HandleAsync)
            .WithName("GetOrderTracking")
            .WithTags("Orders")
            .WithSummary("Track an order")
            .WithDescription("""
                The order as the guest who placed it follows it in the customer app: number, table, status with the
                time of every change, lines and total, without bartender details. No token is needed; the order id
                (returned when the order is placed) is not guessable. `/hubs/order-tracking` says when to reload it.

                - `404 Order.NotFound`: the order does not exist.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Ok<GetOrderTrackingResponse>, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        GetOrderTrackingQuery query = new(id);

        Result<OrderTrackingDto> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetOrderTrackingResponse(result.Value))
            : result.ToProblemDetails();
    }
}
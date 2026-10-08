using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Orders;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Orders;

internal sealed record GetOrderResponse(OrderDto Data);

internal sealed class GetOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/orders/{id:guid}", HandleAsync)
            .WithName("GetOrder")
            .WithTags("Orders");
    }

    private static async Task<Results<Ok<GetOrderResponse>, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        GetOrderQuery query = new(id);

        Result<OrderDto> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetOrderResponse(result.Value))
            : result.ToProblemDetails();
    }
}
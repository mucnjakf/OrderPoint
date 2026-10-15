using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Stock;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Stock;

/// <summary>One stock item.</summary>
/// <param name="Data">The stock item.</param>
internal sealed record GetStockItemResponse(StockItemDto Data);

internal sealed class GetStockItemEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/stock-items/{id:guid}", HandleAsync)
            .WithName("GetStockItem")
            .WithTags("Stock")
            .WithSummary("Get a stock item")
            .WithDescription("""
                Returns one stock item with its quantity, value and level.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<GetStockItemResponse>, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        GetStockItemQuery query = new(id);

        Result<StockItemDto> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetStockItemResponse(result.Value))
            : result.ToProblemDetails();
    }
}
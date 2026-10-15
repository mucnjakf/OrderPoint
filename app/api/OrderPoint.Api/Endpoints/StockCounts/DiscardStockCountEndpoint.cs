using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.StockCounts;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.StockCounts;

internal sealed class DiscardStockCountEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapDelete("api/stock-counts/{id:guid}", HandleAsync)
            .WithName("DiscardStockCount")
            .WithTags("Stock counts")
            .WithSummary("Discard a stock count")
            .WithDescription("""
                Deletes an open count and what was entered on it; the stock stays as it is.

                - `409 StockCount.NotOpen`: the count is closed, so it changed the stock and stays
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
        Result result = await sender.Send(new DiscardStockCountCommand(id), cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }
}
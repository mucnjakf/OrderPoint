using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.StockCounts;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.StockCounts;

/// <summary>One stock count.</summary>
/// <param name="Data">The count with its lines.</param>
internal sealed record GetStockCountResponse(StockCountDto Data);

internal sealed class GetStockCountEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/stock-counts/{id:guid}", HandleAsync)
            .WithName("GetStockCount")
            .WithTags("Stock counts")
            .WithSummary("Get a stock count")
            .WithDescription("""
                Returns one stock count with its lines. While it is open, a line not counted yet expects what is on
                the shelf now.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<GetStockCountResponse>, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        Result<StockCountDto> result = await sender.Send(new GetStockCountQuery(id), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetStockCountResponse(result.Value))
            : result.ToProblemDetails();
    }
}
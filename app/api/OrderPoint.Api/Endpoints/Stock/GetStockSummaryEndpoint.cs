using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Stock;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Stock;

/// <summary>The stock at a glance.</summary>
/// <param name="Data">The total value and how many stock items are low or out.</param>
internal sealed record GetStockSummaryResponse(StockSummaryDto Data);

internal sealed class GetStockSummaryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/stock-items/summary", HandleAsync)
            .WithName("GetStockSummary")
            .WithTags("Stock")
            .WithSummary("Get the stock summary")
            .WithDescription("""
                The value of what is on the shelves (at pack cost) and how many stock items are low or out, over
                active stock items, for the Admin's highlight cards above the stock sheet.
                """)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<GetStockSummaryResponse>, ProblemHttpResult>> HandleAsync(
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        Result<StockSummaryDto> result = await sender.Send(new GetStockSummaryQuery(), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetStockSummaryResponse(result.Value))
            : result.ToProblemDetails();
    }
}
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Stock;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Stock;

/// <summary>What to order again.</summary>
/// <param name="Data">One group per supplier; stock items without a supplier last.</param>
internal sealed record GetReorderListResponse(IReadOnlyList<ReorderGroupDto> Data);

internal sealed class GetReorderListEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/stock-items/reorder", HandleAsync)
            .WithName("GetReorderList")
            .WithTags("Stock")
            .WithSummary("Get the reorder list")
            .WithDescription("""
                Every active stock item at or below its par level (or out), grouped by its supplier, with the whole
                packs that bring it back to twice the par level (at least one) and their cost at the current pack cost.
                """)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<GetReorderListResponse>, ProblemHttpResult>> HandleAsync(
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<ReorderGroupDto>> result = await sender.Send(new GetReorderListQuery(), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetReorderListResponse(result.Value))
            : result.ToProblemDetails();
    }
}
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Tables;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Tables;

/// <summary>The tables with open orders.</summary>
/// <param name="Data">One entry per table with at least one pending, accepted or active order.</param>
internal sealed record GetTablesStatusResponse(IReadOnlyList<TableStatusDto> Data);

internal sealed class GetTablesStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/tables/status", HandleAsync)
            .WithName("GetTablesStatus")
            .WithTags("Tables")
            .WithSummary("Get the open orders per table")
            .WithDescription("""
                For the bar app's tables map: every table with open orders, with how many are pending and in
                progress (accepted or active) and when the longest waiting one was placed. Tables without open orders
                are left out. Not cached; `OrdersChanged` on the staff hub says when to reload it.
                """)
            .RequireAuthorization(AuthorizationPolicies.Bartender);
    }

    private static async Task<Results<Ok<GetTablesStatusResponse>, ProblemHttpResult>> HandleAsync(
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        GetTablesStatusQuery query = new();

        Result<IReadOnlyList<TableStatusDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetTablesStatusResponse(result.Value))
            : result.ToProblemDetails();
    }
}
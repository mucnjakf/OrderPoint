using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.TableRequests;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.TableRequests;

/// <summary>The open requests.</summary>
/// <param name="Data">Every open request of every table, oldest first.</param>
internal sealed record GetTableRequestsResponse(IReadOnlyList<TableRequestDto> Data);

internal sealed class GetTableRequestsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/table-requests", HandleAsync)
            .WithName("GetTableRequests")
            .WithTags("Table requests")
            .WithSummary("Get the open requests")
            .WithDescription("""
                For the bartender app: every guest request no bartender has done yet, oldest first. Not cached;
                `TablesChanged` on the staff hub says when to reload it.
                """)
            .RequireAuthorization(AuthorizationPolicies.Bartender);
    }

    private static async Task<Results<Ok<GetTableRequestsResponse>, ProblemHttpResult>> HandleAsync(
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        GetTableRequestsQuery query = new();

        Result<IReadOnlyList<TableRequestDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetTableRequestsResponse(result.Value))
            : result.ToProblemDetails();
    }
}
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.TableRequests;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.TableRequests;

/// <summary>The table's open requests.</summary>
/// <param name="Data">At most one of each type, oldest first.</param>
internal sealed record GetTableRequestsByCodeResponse(IReadOnlyList<TableRequestDto> Data);

internal sealed class GetTableRequestsByCodeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/tables/by-code/{code}/requests", HandleAsync)
            .WithName("GetTableRequestsByCode")
            .WithTags("Table requests")
            .WithSummary("Get a table's open requests")
            .WithDescription("""
                What the guests at the table asked for that no bartender has done yet. No token is needed; not
                cached, `TablesChanged` on `/hubs/order-tracking` says when to reload it.

                - `404 Table.NotFound`: no table has this code
                """)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Ok<GetTableRequestsByCodeResponse>, ProblemHttpResult>> HandleAsync(
        [FromRoute] string code,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        GetTableRequestsByCodeQuery query = new(code);

        Result<IReadOnlyList<TableRequestDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetTableRequestsByCodeResponse(result.Value))
            : result.ToProblemDetails();
    }
}
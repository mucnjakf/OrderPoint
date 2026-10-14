using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Tables;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Tables;

/// <summary>One table.</summary>
/// <param name="Data">The table.</param>
internal sealed record GetTableResponse(TableDto Data);

internal sealed class GetTableEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/tables/{id:guid}", HandleAsync)
            .WithName("GetTable")
            .WithTags("Tables")
            .WithSummary("Get a table")
            .WithDescription("""
                Returns one table with its place on the floor plan.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthorizationPolicies.AdminOrBartender);
    }

    private static async Task<Results<Ok<GetTableResponse>, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        GetTableQuery query = new(id);

        Result<TableDto> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetTableResponse(result.Value))
            : result.ToProblemDetails();
    }
}
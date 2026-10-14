using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Tables;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Tables;

/// <summary>The scanned table.</summary>
/// <param name="Data">The table as guests see it.</param>
internal sealed record GetTableByCodeResponse(GuestTableDto Data);

internal sealed class GetTableByCodeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/tables/by-code/{code}", HandleAsync)
            .WithName("GetTableByCode")
            .WithTags("Tables")
            .WithSummary("Look up a table by its code")
            .WithDescription("""
                The customer app checks the code from a table's QR code before it remembers the table: the code, the
                area's name and whether the table takes orders. No token is needed.

                - `404 Table.NotFound`: no table has this code
                """)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Ok<GetTableByCodeResponse>, ProblemHttpResult>> HandleAsync(
        [FromRoute] string code,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        GetTableByCodeQuery query = new(code);

        Result<GuestTableDto> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetTableByCodeResponse(result.Value))
            : result.ToProblemDetails();
    }
}
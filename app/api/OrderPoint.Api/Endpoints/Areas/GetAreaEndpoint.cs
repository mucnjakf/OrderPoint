using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Areas;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Areas;

/// <summary>One area.</summary>
/// <param name="Data">The area with its tables and fixtures.</param>
internal sealed record GetAreaResponse(AreaDto Data);

internal sealed class GetAreaEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/areas/{id:guid}", HandleAsync)
            .WithName("GetArea")
            .WithTags("Areas")
            .WithSummary("Get an area")
            .WithDescription("""
                Returns one area with its tables (by code) and fixtures.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthorizationPolicies.AdminOrBartender);
    }

    private static async Task<Results<Ok<GetAreaResponse>, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        GetAreaQuery query = new(id);

        Result<AreaDto> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetAreaResponse(result.Value))
            : result.ToProblemDetails();
    }
}
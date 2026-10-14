using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Fixtures;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Fixtures;

/// <summary>One fixture.</summary>
/// <param name="Data">The fixture.</param>
internal sealed record GetFixtureResponse(FixtureDto Data);

internal sealed class GetFixtureEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/fixtures/{id:guid}", HandleAsync)
            .WithName("GetFixture")
            .WithTags("Fixtures")
            .WithSummary("Get a fixture")
            .WithDescription("""
                Returns one fixture with its place on the floor plan.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthorizationPolicies.AdminOrBartender);
    }

    private static async Task<Results<Ok<GetFixtureResponse>, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        GetFixtureQuery query = new(id);

        Result<FixtureDto> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetFixtureResponse(result.Value))
            : result.ToProblemDetails();
    }
}
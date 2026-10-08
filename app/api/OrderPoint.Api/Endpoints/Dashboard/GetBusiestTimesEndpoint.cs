using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Dashboard;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Dashboard;

internal sealed record GetBusiestTimesResponse(IReadOnlyList<BusiestTimeDto> Data);

internal sealed class GetBusiestTimesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/dashboard/busiest-times", HandleAsync)
            .WithName("GetBusiestTimes")
            .WithTags("Dashboard")
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<GetBusiestTimesResponse>, ProblemHttpResult>> HandleAsync(
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        GetBusiestTimesQuery query = new();

        Result<IReadOnlyList<BusiestTimeDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetBusiestTimesResponse(result.Value))
            : result.ToProblemDetails();
    }
}
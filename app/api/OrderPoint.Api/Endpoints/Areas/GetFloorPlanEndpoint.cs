using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Areas;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Areas;

/// <summary>The floor plan.</summary>
/// <param name="Data">Every area in tab order with its tables and fixtures.</param>
internal sealed record GetFloorPlanResponse(IReadOnlyList<AreaDto> Data);

internal sealed class GetFloorPlanEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/floor-plan", HandleAsync)
            .WithName("GetFloorPlan")
            .WithTags("Areas")
            .WithSummary("Get the floor plan")
            .WithDescription("""
                Returns every area in tab order (`displayOrder`) with its tables (by code) and fixtures. A bar has
                few, so it is one response, for the Admin's floor plan editor and the bar app's tables map.
                `TablesChanged` on the staff hub says when to reload it.
                """)
            .RequireAuthorization(AuthorizationPolicies.AdminOrBartender);
    }

    private static async Task<Results<Ok<GetFloorPlanResponse>, ProblemHttpResult>> HandleAsync(
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        GetFloorPlanQuery query = new();

        Result<IReadOnlyList<AreaDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetFloorPlanResponse(result.Value))
            : result.ToProblemDetails();
    }
}
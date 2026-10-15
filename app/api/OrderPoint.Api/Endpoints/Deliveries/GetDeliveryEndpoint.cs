using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Deliveries;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Deliveries;

/// <summary>One delivery.</summary>
/// <param name="Data">The delivery with its lines.</param>
internal sealed record GetDeliveryResponse(DeliveryDto Data);

internal sealed class GetDeliveryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/deliveries/{id:guid}", HandleAsync)
            .WithName("GetDelivery")
            .WithTags("Deliveries")
            .WithSummary("Get a delivery")
            .WithDescription("""
                Returns one delivery with its lines.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<GetDeliveryResponse>, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        Result<DeliveryDto> result = await sender.Send(new GetDeliveryQuery(id), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetDeliveryResponse(result.Value))
            : result.ToProblemDetails();
    }
}
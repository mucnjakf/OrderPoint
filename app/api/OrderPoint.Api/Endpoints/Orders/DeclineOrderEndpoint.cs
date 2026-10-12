using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Orders;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Orders;

internal sealed class DeclineOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/orders/{id:guid}/decline", HandleAsync)
            .WithName("DeclineOrder")
            .WithTags("Orders")
            .WithSummary("Decline an order")
            .WithDescription("""
                Declines an order and records the signed-in bartender. Any bartender can decline a pending order; an
                accepted order only the bartender who accepted it.

                - `404 Bartender.NotFound`: the signed-in bartender no longer exists
                - `409 Order.InvalidStatusTransition`: the order is neither pending nor accepted
                - `409 Order.AssignedToAnotherBartender`: the order was accepted by another bartender
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthorizationPolicies.Bartender);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        DeclineOrderCommand command = new(id, user.GetUserId());

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }
}
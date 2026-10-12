using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Orders;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Orders;

internal sealed class ActivateOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/orders/{id:guid}/activate", HandleAsync)
            .WithName("ActivateOrder")
            .WithTags("Orders")
            .WithSummary("Start preparing an order")
            .WithDescription("""
                Moves an accepted order to Active: the bartender who accepted it starts preparing it.

                - `409 Order.InvalidStatusTransition`: the order is not accepted
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
        ActivateOrderCommand command = new(id, user.GetUserId());

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }
}
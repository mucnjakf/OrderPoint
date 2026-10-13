using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Orders;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Orders;

internal sealed class CompleteOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/orders/{id:guid}/complete", HandleAsync)
            .WithName("CompleteOrder")
            .WithTags("Orders")
            .WithSummary("Complete an order")
            .WithDescription("""
                Moves an active order to Completed: the bartender who prepared it has served it.

                - `403 Auth.AccountInactive`: the signed-in bartender was deactivated
                - `404 Bartender.NotFound`: the signed-in bartender no longer exists
                - `409 Order.InvalidStatusTransition`: the order is not active
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
        CompleteOrderCommand command = new(id, user.GetUserId());

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }
}
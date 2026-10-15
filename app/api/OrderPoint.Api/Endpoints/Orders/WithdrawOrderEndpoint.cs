using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Orders;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Orders;

internal sealed class WithdrawOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/orders/{id:guid}/withdraw", HandleAsync)
            .WithName("WithdrawOrder")
            .WithTags("Orders")
            .WithSummary("Withdraw an order")
            .WithDescription("""
                The bartender who took an order in the Bartender app takes it back while it is still pending, e.g.
                when the guest changed their mind. It becomes `Cancelled`; it was not paid, so nothing is refunded.

                - `403 Auth.AccountInactive`: the signed-in bartender was deactivated
                - `403 Auth.AccountDeleted`: the signed-in bartender no longer exists
                - `404 Order.NotFound`: the order does not exist
                - `409 Order.EnteredByAnotherBartender`: another bartender took the order (or a guest placed it)
                - `409 Order.InvalidStatusTransition`: the order is not pending (the bar already accepted it)
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
        WithdrawOrderCommand command = new(id, user.GetUserId());

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }
}
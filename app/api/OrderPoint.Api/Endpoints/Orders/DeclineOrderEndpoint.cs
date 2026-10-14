using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Orders;
using OrderPoint.Domain.Enumerations;
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
                Declines an order. A bartender declines a pending order (any bartender) or one they accepted, and is
                recorded on it. The admin can decline any open order (pending, accepted or active), e.g. one nobody
                will finish at closing time; its bartender, if any, stays recorded.

                - `403 Auth.AccountInactive`: the signed-in bartender was deactivated
                - `404 Order.NotFound`: the order does not exist
                - `404 Bartender.NotFound`: the signed-in bartender no longer exists
                - `409 Order.InvalidStatusTransition`: the order is not open (for a bartender: neither pending nor
                  accepted)
                - `409 Order.AssignedToAnotherBartender`: the order was accepted by another bartender
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthorizationPolicies.AdminOrBartender);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        Result result = user.IsInRole(nameof(UserRole.Admin))
            ? await sender.Send(new DeclineOrderAsAdminCommand(id), cancellationToken)
            : await sender.Send(new DeclineOrderCommand(id, user.GetUserId()), cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }
}
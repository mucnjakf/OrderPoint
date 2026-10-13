using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Orders;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Orders;

internal sealed class AcceptOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/orders/{id:guid}/accept", HandleAsync)
            .WithName("AcceptOrder")
            .WithTags("Orders")
            .WithSummary("Accept an order")
            .WithDescription("""
                Accepts a pending order and assigns it to the signed-in bartender, who is then the only one who can
                start, complete or decline it.

                - `403 Auth.AccountInactive`: the signed-in bartender was deactivated
                - `404 Bartender.NotFound`: the signed-in bartender no longer exists
                - `409 Order.InvalidStatusTransition`: the order is not pending (e.g. another bartender accepted it
                  first)
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
        AcceptOrderCommand command = new(id, user.GetUserId());

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }
}
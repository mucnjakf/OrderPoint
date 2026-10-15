using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Orders;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Orders;

/// <summary>Why the order is declined.</summary>
/// <param name="Reason">The reason, shown to the guest, e.g. "Out of stock".</param>
internal sealed record DeclineOrderRequest(string Reason);

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
                will finish at closing time; its bartender, if any, stays recorded. The reason is shown to the guest.
                A paid order is refunded: a card at once (`paymentStatus` `Refunded`), cash at the register
                (`RefundDue`).

                - `403 Auth.AccountInactive`: the signed-in bartender was deactivated
                - `403 Auth.AccountDeleted`: the signed-in bartender no longer exists
                - `404 Order.NotFound`: the order does not exist
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
        [FromBody] DeclineOrderRequest request,
        [FromServices] IValidator<DeclineOrderRequest> validator,
        [FromServices] ISender sender,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        Result result = user.IsInRole(nameof(UserRole.Admin))
            ? await sender.Send(new DeclineOrderAsAdminCommand(id, request.Reason), cancellationToken)
            : await sender.Send(new DeclineOrderCommand(id, user.GetUserId(), request.Reason), cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class DeclineOrderRequestValidator : AbstractValidator<DeclineOrderRequest>
    {
        public DeclineOrderRequestValidator()
        {
            RuleFor(request => request.Reason)
                .NotEmpty().WithMessage("Reason is required")
                .MaximumLength(200).WithMessage("Reason must be at most 200 characters");
        }
    }
}
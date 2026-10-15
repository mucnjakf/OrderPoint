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

/// <summary>A payment at the register.</summary>
/// <param name="Method">`Cash` or `Card` (charged on the bar's own card terminal; only recorded here).</param>
/// <param name="CashReceived">For `Cash`, what the guest handed over, in euros, at least the total; empty for `Card`.
/// </param>
internal sealed record PayOrderRequest(PaymentMethod Method, decimal? CashReceived);

internal sealed class PayOrderEndpoint : IEndpoint
{
    // Plenty for a bar's till, and keeps typos like 2000 for 20.00 out
    internal const decimal MaxCashReceived = 1000;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/orders/{id:guid}/pay", HandleAsync)
            .WithName("PayOrder")
            .WithTags("Orders")
            .WithSummary("Pay an order at the register")
            .WithDescription("""
                The guest pays an order a bartender took in the Bartender app at the register: in cash (with what they
                handed over, so the receipt shows the change) or by card on the bar's own terminal. The signed-in
                bartender is recorded as `paidByBartender`. `cashReceived` is required for `Cash` and empty for
                `Card`.

                - `403 Auth.AccountInactive`: the signed-in bartender was deactivated
                - `403 Auth.AccountDeleted`: the signed-in bartender no longer exists
                - `400 Payment.NotEnoughCash`: the cash received is less than the total
                - `404 Order.NotFound`: the order does not exist
                - `409 Payment.AlreadyPaid`: the order is already paid
                - `409 Payment.NothingToPay`: the order was declined or cancelled
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthorizationPolicies.Bartender);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromBody] PayOrderRequest request,
        [FromServices] IValidator<PayOrderRequest> validator,
        [FromServices] ISender sender,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        PayOrderCommand command = new(id, request.Method, request.CashReceived, user.GetUserId());

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class PayOrderRequestValidator : AbstractValidator<PayOrderRequest>
    {
        public PayOrderRequestValidator()
        {
            RuleFor(request => request.Method)
                .IsInEnum().WithMessage("Method must be Cash or Card");

            RuleFor(request => request.CashReceived)
                .NotNull().WithMessage("CashReceived is required for a cash payment")
                .GreaterThan(0).WithMessage("CashReceived must be positive")
                .LessThanOrEqualTo(MaxCashReceived).WithMessage($"CashReceived must be at most {MaxCashReceived}")
                .When(request => request.Method == PaymentMethod.Cash);

            RuleFor(request => request.CashReceived)
                .Null().WithMessage("CashReceived must be empty for a card payment")
                .When(request => request.Method == PaymentMethod.Card);
        }
    }
}
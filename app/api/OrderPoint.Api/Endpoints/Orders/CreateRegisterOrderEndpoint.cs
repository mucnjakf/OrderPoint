using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Orders;
using OrderPoint.Application.Dtos;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Orders;

/// <summary>A sale at the register: an order at the counter, paid in the same step.</summary>
/// <param name="CustomerName">Who to call at the counter, e.g. `Ana`, or `null`.</param>
/// <param name="Note">An optional note for the bar, e.g. "no ice".</param>
/// <param name="Items">The order lines; each item at most once.</param>
/// <param name="Payment">How the guest pays.</param>
internal sealed record CreateRegisterOrderRequest(
    string? CustomerName,
    string? Note,
    IReadOnlyList<CreateOrderItemRequest> Items,
    PayOrderRequest Payment);

/// <summary>The sale, paid.</summary>
/// <param name="Data">The order, for the receipt.</param>
internal sealed record CreateRegisterOrderResponse(OrderDto Data);

internal sealed class CreateRegisterOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPost("api/orders/register", HandleAsync)
            .WithName("CreateRegisterOrder")
            .WithTags("Orders")
            .WithSummary("Sell at the register")
            .WithDescription("""
                A sale at the register: an order at the counter (with an optional `customerName` to call out) that is
                paid right away, in cash or by card on the bar's own terminal, and goes to the bar like any other
                order (`Pending`, the next order `number`, current prices). The signed-in bartender is recorded as
                `takenByBartender` and `paidByBartender`. Same limits as a guest's order: at most 50 lines, each item
                at most once and at most 20 pieces.

                - `403 Auth.AccountInactive`: the signed-in bartender was deactivated
                - `403 Auth.AccountDeleted`: the signed-in bartender no longer exists
                - `400 Payment.NotEnoughCash`: the cash received is less than the total
                - `404 Item.NotFound`: an item does not exist.
                - `409 Order.ItemIsUnavailable`: an item or its category is inactive.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthorizationPolicies.Bartender);
    }

    private static async Task<Results<CreatedAtRoute<CreateRegisterOrderResponse>, ProblemHttpResult>> HandleAsync(
        [FromBody] CreateRegisterOrderRequest request,
        [FromServices] IValidator<CreateRegisterOrderRequest> validator,
        [FromServices] ISender sender,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        CreateRegisterOrderCommand command = new(
            request.CustomerName,
            request.Note,
            request.Items.Select(item => new CreateOrderItemCommand(item.ItemId, item.Quantity)).ToList(),
            request.Payment.Method,
            request.Payment.CashReceived,
            user.GetUserId());

        Result<OrderDto> result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(
                new CreateRegisterOrderResponse(result.Value),
                "GetOrder",
                new { id = result.Value.Id })
            : result.ToProblemDetails();
    }

    internal sealed class CreateRegisterOrderRequestValidator : AbstractValidator<CreateRegisterOrderRequest>
    {
        public CreateRegisterOrderRequestValidator()
        {
            RuleFor(request => request.CustomerName)
                .MaximumLength(30).WithMessage("CustomerName must be at most 30 characters");

            RuleFor(request => request.Note)
                .MaximumLength(200).WithMessage("Note must be at most 200 characters");

            RuleFor(request => request.Items)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Items are required")
                .Must(items => items.Count <= 50).WithMessage("Items must be at most 50")
                .Must(items => items.DistinctBy(item => item.ItemId).Count() == items.Count)
                .WithMessage("Items must be unique");

            RuleForEach(request => request.Items).ChildRules(item =>
            {
                item.RuleFor(orderItem => orderItem.ItemId)
                    .NotEmpty().WithMessage("ItemId is required");

                item.RuleFor(orderItem => orderItem.Quantity)
                    .InclusiveBetween(1, 20).WithMessage("Quantity must be between 1 and 20");
            });

            RuleFor(request => request.Payment)
                .NotNull().WithMessage("Payment is required")
                .SetValidator(new PayOrderEndpoint.PayOrderRequestValidator());
        }
    }
}
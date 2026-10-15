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

/// <summary>An order a bartender takes at a table or at the counter.</summary>
/// <param name="TableCode">The code of the table, e.g. `T11`, or `null` for an order at the counter.</param>
/// <param name="CustomerName">Who to call at the counter, e.g. `Ana`; ignored for a table.</param>
/// <param name="Note">An optional note for the bar, e.g. "no ice".</param>
/// <param name="Items">The order lines; each item at most once.</param>
internal sealed record CreateBartenderOrderRequest(
    string? TableCode,
    string? CustomerName,
    string? Note,
    IReadOnlyList<CreateOrderItemRequest> Items);

/// <summary>The order the bartender took.</summary>
/// <param name="Data">The order.</param>
internal sealed record CreateBartenderOrderResponse(OrderDto Data);

internal sealed class CreateBartenderOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPost("api/orders/bartender", HandleAsync)
            .WithName("CreateBartenderOrder")
            .WithTags("Orders")
            .WithSummary("Take an order")
            .WithDescription("""
                The signed-in bartender takes an order in the Bartender app, at a table or, without a `tableCode`,
                at the counter (with an optional `customerName` to call out). It goes to the bar like a guest's order:
                `Pending`, the next order `number`, current prices. It is not paid yet (`paymentStatus` `Unpaid`): the
                guest pays at the register. The bartender is recorded as `takenByBartender`.

                Same limits as a guest's order: at most 50 lines, each item at most once and at most 20 pieces.

                - `403 Auth.AccountInactive`: the signed-in bartender was deactivated
                - `403 Auth.AccountDeleted`: the signed-in bartender no longer exists
                - `404 Table.NotFound`: no table has this code.
                - `404 Item.NotFound`: an item does not exist.
                - `409 Table.IsInactive`: the table is not taking orders.
                - `409 Order.ItemIsUnavailable`: an item or its category is inactive.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthorizationPolicies.Bartender);
    }

    private static async Task<Results<CreatedAtRoute<CreateBartenderOrderResponse>, ProblemHttpResult>> HandleAsync(
        [FromBody] CreateBartenderOrderRequest request,
        [FromServices] IValidator<CreateBartenderOrderRequest> validator,
        [FromServices] ISender sender,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        CreateBartenderOrderCommand command = new(
            request.TableCode,
            request.CustomerName,
            request.Note,
            request.Items.Select(item => new CreateOrderItemCommand(item.ItemId, item.Quantity)).ToList(),
            user.GetUserId());

        Result<OrderDto> result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(
                new CreateBartenderOrderResponse(result.Value),
                "GetOrder",
                new { id = result.Value.Id })
            : result.ToProblemDetails();
    }

    internal sealed class CreateBartenderOrderRequestValidator : AbstractValidator<CreateBartenderOrderRequest>
    {
        public CreateBartenderOrderRequestValidator()
        {
            RuleFor(request => request.TableCode)
                .NotEmpty().WithMessage("TableCode must not be empty; leave it out for the counter")
                .MaximumLength(10).WithMessage("TableCode must be at most 10 characters")
                .Matches("^[A-Za-z0-9-]+$").WithMessage("TableCode may only contain letters, digits and hyphens")
                .When(request => request.TableCode is not null);

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
        }
    }
}
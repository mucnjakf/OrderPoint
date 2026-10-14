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

/// <summary>One order line.</summary>
/// <param name="ItemId">The item to order. It and its category must be active.</param>
/// <param name="Quantity">How many pieces, at least 1.</param>
internal sealed record CreateOrderItemRequest(Guid ItemId, int Quantity);

/// <summary>The order placed from a table.</summary>
/// <param name="TableCode">The code of the table the order is placed from, e.g. `T11` (from the QR code).</param>
/// <param name="Note">An optional note for the bartender, e.g. "no ice".</param>
/// <param name="Items">The order lines; each item at most once.</param>
internal sealed record CreateOrderRequest(
    string TableCode,
    string? Note,
    IReadOnlyList<CreateOrderItemRequest> Items);

/// <summary>The placed order.</summary>
/// <param name="Data">The order.</param>
internal sealed record CreateOrderResponse(OrderDto Data);

internal sealed class CreateOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPost("api/orders", HandleAsync)
            .WithName("CreateOrder")
            .WithTags("Orders")
            .WithSummary("Place an order")
            .WithDescription("""
                Places an order from a table. Used by the customer app, so no token is needed. Every line copies the
                item's current price into `unitPrice`; the order starts as `Pending` and gets the next order `number`.

                Limits, so an anonymous caller can't place absurd orders: at most 50 lines, each item at most once and
                at most 20 pieces, and a table code of up to 10 letters, digits or hyphens.

                - `404 Table.NotFound`: no table has this code.
                - `404 Item.NotFound`: an item does not exist.
                - `409 Table.IsInactive`: the table is not taking orders.
                - `409 Order.ItemIsUnavailable`: an item or its category is inactive.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<Results<CreatedAtRoute<CreateOrderResponse>, ProblemHttpResult>> HandleAsync(
        [FromBody] CreateOrderRequest request,
        [FromServices] IValidator<CreateOrderRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        CreateOrderCommand command = new(
            request.TableCode,
            request.Note,
            request.Items.Select(item => new CreateOrderItemCommand(item.ItemId, item.Quantity)).ToList());

        Result<OrderDto> result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(
                new CreateOrderResponse(result.Value),
                "GetOrder",
                new { id = result.Value.Id })
            : result.ToProblemDetails();
    }

    internal sealed class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
    {
        public CreateOrderRequestValidator()
        {
            RuleFor(request => request.TableCode)
                .NotEmpty().WithMessage("TableCode is required")
                .MaximumLength(10).WithMessage("TableCode must be at most 10 characters")
                .Matches("^[A-Za-z0-9-]+$").WithMessage("TableCode may only contain letters, digits and hyphens");

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
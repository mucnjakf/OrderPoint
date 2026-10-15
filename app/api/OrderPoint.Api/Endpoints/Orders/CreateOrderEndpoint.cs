using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Orders;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Payments;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Orders;

/// <summary>One order line.</summary>
/// <param name="ItemId">The item to order. It and its category must be active.</param>
/// <param name="Quantity">How many pieces, at least 1.</param>
internal sealed record CreateOrderItemRequest(Guid ItemId, int Quantity);

/// <summary>The card the order is paid with. Payments are simulated: nothing is charged or stored.</summary>
/// <param name="Number">The card number, 13 to 19 digits, spaces allowed. `4000 0000 0000 0002` is declined.</param>
/// <param name="Expiry">The expiry as `MM/YY`, this month or later.</param>
/// <param name="Cvc">The 3 or 4 digit security code.</param>
internal sealed record CreateOrderCardRequest(string Number, string Expiry, string Cvc);

/// <summary>The order placed from a table.</summary>
/// <param name="TableCode">The code of the table the order is placed from, e.g. `T11` (from the QR code).</param>
/// <param name="Note">An optional note for the bartender, e.g. "no ice".</param>
/// <param name="Items">The order lines; each item at most once.</param>
/// <param name="Card">The card that pays the order, charged before the order is placed.</param>
internal sealed record CreateOrderRequest(
    string TableCode,
    string? Note,
    IReadOnlyList<CreateOrderItemRequest> Items,
    CreateOrderCardRequest Card);

/// <summary>The placed order.</summary>
/// <param name="Data">The order.</param>
internal sealed record CreateOrderResponse(OrderDto Data);

internal sealed class CreateOrderEndpoint : IEndpoint
{
    private const string CardNumberPattern = "^[0-9 ]+$";

    private const string ExpiryPattern = "^(0[1-9]|1[0-2])/[0-9]{2}$";

    private const string CvcPattern = "^[0-9]{3,4}$";

    private const int MinimumCardNumberDigits = 13;

    private const int MaximumCardNumberDigits = 19;

    // MM/YY gives only the last two digits of the year
    private const int ExpiryCenturyStartYear = 2000;

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

                The guest pays when ordering: the `card` is charged for the total first, and the order is placed as
                `paymentStatus` `Paid` by `Card`. Payments are simulated, so nothing is charged or stored: every
                well-formed card is approved except `4000 0000 0000 0002` (use `4242 4242 4242 4242` for a working
                one). Declining or cancelling the order later refunds the card.

                Limits, so an anonymous caller can't place absurd orders: at most 50 lines, each item at most once and
                at most 20 pieces, and a table code of up to 10 letters, digits or hyphens.

                - `400 Payment.CardDeclined`: the card was declined; no order was placed.
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
            request.Items.Select(item => new CreateOrderItemCommand(item.ItemId, item.Quantity)).ToList(),
            new CardDetails(request.Card.Number, request.Card.Expiry, request.Card.Cvc));

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

            RuleFor(request => request.Card)
                .NotNull().WithMessage("Card is required");

            RuleFor(request => request.Card.Number)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Card number is required")
                .Matches(CardNumberPattern).WithMessage("Card number may only contain digits and spaces")
                .Must(HaveCardNumberLength)
                .WithMessage($"Card number must have {MinimumCardNumberDigits} to {MaximumCardNumberDigits} digits")
                .When(request => request.Card is not null);

            RuleFor(request => request.Card.Expiry)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Card expiry is required")
                .Matches(ExpiryPattern).WithMessage("Card expiry must be MM/YY")
                .Must(NotBeExpired).WithMessage("Card has expired")
                .When(request => request.Card is not null);

            RuleFor(request => request.Card.Cvc)
                .NotEmpty().WithMessage("Card CVC is required")
                .Matches(CvcPattern).WithMessage("Card CVC must be 3 or 4 digits")
                .When(request => request.Card is not null);

            RuleForEach(request => request.Items).ChildRules(item =>
            {
                item.RuleFor(orderItem => orderItem.ItemId)
                    .NotEmpty().WithMessage("ItemId is required");

                item.RuleFor(orderItem => orderItem.Quantity)
                    .InclusiveBetween(1, 20).WithMessage("Quantity must be between 1 and 20");
            });
        }

        private static bool HaveCardNumberLength(string number)
        {
            int digitCount = number.Count(char.IsDigit);

            return digitCount is >= MinimumCardNumberDigits and <= MaximumCardNumberDigits;
        }

        // Valid until the end of its month
        private static bool NotBeExpired(string expiry)
        {
            int month = int.Parse(expiry[..2]);
            int year = ExpiryCenturyStartYear + int.Parse(expiry[3..]);

            DateTimeOffset now = DateTimeOffset.UtcNow;

            return year > now.Year || (year == now.Year && month >= now.Month);
        }
    }
}
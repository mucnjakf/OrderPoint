using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Endpoints.Stock;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Deliveries;
using OrderPoint.Application.Dtos;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Deliveries;

/// <summary>One stock item that came.</summary>
/// <param name="StockItemId">The stock item.</param>
/// <param name="Packs">How many packs came, e.g. 2 (kegs).</param>
/// <param name="PackCost">What one pack cost on this delivery, in euros.</param>
internal sealed record CreateDeliveryLineRequest(Guid StockItemId, decimal Packs, decimal PackCost);

/// <summary>What a supplier delivered.</summary>
/// <param name="SupplierId">Who delivered it.</param>
/// <param name="DeliveredOn">The day the goods came, e.g. `2026-10-15`; not later than tomorrow.</param>
/// <param name="Reference">The invoice or delivery note number; optional.</param>
/// <param name="Note">A note; optional.</param>
/// <param name="Lines">The stock items that came, each at most once.</param>
internal sealed record CreateDeliveryRequest(
    Guid SupplierId,
    DateOnly DeliveredOn,
    string? Reference,
    string? Note,
    IReadOnlyList<CreateDeliveryLineRequest> Lines);

/// <summary>The recorded delivery.</summary>
/// <param name="Data">The delivery.</param>
internal sealed record CreateDeliveryResponse(DeliveryDto Data);

internal sealed class CreateDeliveryEndpoint : IEndpoint
{
    private const int MaxLines = 50;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPost("api/deliveries", HandleAsync)
            .WithName("CreateDelivery")
            .WithTags("Deliveries")
            .WithSummary("Record a delivery")
            .WithDescription($"""
                Records what a supplier delivered. Every line adds its packs to the stock item as a `Delivery`
                movement valued at this delivery's pack cost, and the stock item's pack cost becomes the average of
                what was on the shelf (stock below 0 counts as none) and what came. A delivery can't be changed later;
                a mistake is fixed with a correction. Items sold out for lack of these stock items come back. At most
                {MaxLines} lines, each stock item at most once; the day can be at most tomorrow (time zones).

                - `404 Supplier.NotFound`: the supplier does not exist
                - `404 StockItem.NotFound`: a stock item does not exist
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<CreatedAtRoute<CreateDeliveryResponse>, ProblemHttpResult>> HandleAsync(
        [FromBody] CreateDeliveryRequest request,
        [FromServices] IValidator<CreateDeliveryRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        CreateDeliveryCommand command = new(
            request.SupplierId,
            request.DeliveredOn,
            request.Reference,
            request.Note,
            request.Lines
                .Select(line => new CreateDeliveryLineCommand(line.StockItemId, line.Packs, line.PackCost))
                .ToList());

        Result<DeliveryDto> result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(
                new CreateDeliveryResponse(result.Value),
                "GetDelivery",
                new { id = result.Value.Id })
            : result.ToProblemDetails();
    }

    internal sealed class CreateDeliveryRequestValidator : AbstractValidator<CreateDeliveryRequest>
    {
        public CreateDeliveryRequestValidator()
        {
            RuleFor(request => request.SupplierId)
                .NotEmpty().WithMessage("SupplierId is required");

            RuleFor(request => request.DeliveredOn)
                .Must(deliveredOn => deliveredOn <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
                .WithMessage("DeliveredOn can't be in the future");

            RuleFor(request => request.Reference)
                .MaximumLength(50).WithMessage("Reference must be at most 50 characters");

            RuleFor(request => request.Note)
                .MaximumLength(200).WithMessage("Note must be at most 200 characters");

            RuleFor(request => request.Lines)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Lines are required")
                .Must(lines => lines.Count <= MaxLines).WithMessage($"Lines must be at most {MaxLines}")
                .Must(lines => lines.DistinctBy(line => line.StockItemId).Count() == lines.Count)
                .WithMessage("Each stock item may be on the delivery only once");

            RuleForEach(request => request.Lines).ChildRules(line =>
            {
                line.RuleFor(deliveryLine => deliveryLine.StockItemId)
                    .NotEmpty().WithMessage("StockItemId is required");

                line.RuleFor(deliveryLine => deliveryLine.Packs)
                    .GreaterThan(0).WithMessage("Packs must be positive")
                    .LessThanOrEqualTo(CreateStockItemEndpoint.MaxQuantity)
                    .WithMessage($"Packs must be at most {CreateStockItemEndpoint.MaxQuantity}");

                line.RuleFor(deliveryLine => deliveryLine.PackCost)
                    .GreaterThanOrEqualTo(0).WithMessage("PackCost must not be negative")
                    .LessThanOrEqualTo(CreateStockItemEndpoint.MaxPackCost)
                    .WithMessage($"PackCost must be at most {CreateStockItemEndpoint.MaxPackCost}");
            });
        }
    }
}
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Stock;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Stock;

/// <summary>The stock item's new values; the unit and the quantity stay.</summary>
/// <param name="Name">The name, e.g. "Havana Club 3"; unique.</param>
/// <param name="Group">The group it is listed and counted under.</param>
/// <param name="PackName">What it comes in, e.g. "keg", "bottle", "case".</param>
/// <param name="PackSize">How many base units one pack holds, e.g. 700 (ml) for a bottle.</param>
/// <param name="PackCost">What one pack costs, in euros.</param>
/// <param name="ParLevel">The quantity at or below which it counts as low, in base units.</param>
/// <param name="Status">The status. An inactive stock item is no longer bought.</param>
/// <param name="SupplierId">Who it is usually bought from, for the reorder list; optional.</param>
internal sealed record UpdateStockItemRequest(
    string Name,
    StockGroup Group,
    string PackName,
    decimal PackSize,
    decimal PackCost,
    decimal ParLevel,
    StockItemStatus Status,
    Guid? SupplierId);

internal sealed class UpdateStockItemEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/stock-items/{id:guid}", HandleAsync)
            .WithName("UpdateStockItem")
            .WithTags("Stock")
            .WithSummary("Update a stock item")
            .WithDescription("""
                Updates the name, group, pack, pack cost, par level, status and supplier. The unit can't change, and
                the quantity only changes through movements (`POST api/stock-items/{id}/waste` and `/correction`,
                deliveries, sales). Movements already recorded keep the cost of their time.

                - `404 Supplier.NotFound`: the supplier does not exist
                - `409 StockItem.NameAlreadyExists`: another stock item has this name
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateStockItemRequest request,
        [FromServices] IValidator<UpdateStockItemRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        UpdateStockItemCommand command = new(
            id,
            request.Name,
            request.Group,
            request.PackName,
            request.PackSize,
            request.PackCost,
            request.ParLevel,
            request.Status,
            request.SupplierId);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class UpdateStockItemRequestValidator : AbstractValidator<UpdateStockItemRequest>
    {
        public UpdateStockItemRequestValidator()
        {
            RuleFor(request => request.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name must be at most 100 characters");

            RuleFor(request => request.Group)
                .IsInEnum().WithMessage("Group is invalid");

            RuleFor(request => request.PackName)
                .NotEmpty().WithMessage("PackName is required")
                .MaximumLength(30).WithMessage("PackName must be at most 30 characters");

            RuleFor(request => request.PackSize)
                .GreaterThan(0).WithMessage("PackSize must be positive")
                .LessThanOrEqualTo(CreateStockItemEndpoint.MaxQuantity)
                .WithMessage($"PackSize must be at most {CreateStockItemEndpoint.MaxQuantity}");

            RuleFor(request => request.PackCost)
                .GreaterThanOrEqualTo(0).WithMessage("PackCost must not be negative")
                .LessThanOrEqualTo(CreateStockItemEndpoint.MaxPackCost)
                .WithMessage($"PackCost must be at most {CreateStockItemEndpoint.MaxPackCost}");

            RuleFor(request => request.ParLevel)
                .GreaterThanOrEqualTo(0).WithMessage("ParLevel must not be negative")
                .LessThanOrEqualTo(CreateStockItemEndpoint.MaxQuantity)
                .WithMessage($"ParLevel must be at most {CreateStockItemEndpoint.MaxQuantity}");

            RuleFor(request => request.Status)
                .IsInEnum().WithMessage("Status is invalid");
        }
    }
}
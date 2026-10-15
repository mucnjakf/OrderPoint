using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Stock;
using OrderPoint.Application.Dtos;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Stock;

/// <summary>The new stock item.</summary>
/// <param name="Name">The name, e.g. "Havana Club 3"; unique.</param>
/// <param name="Group">The group it is listed and counted under.</param>
/// <param name="Unit">The base unit every quantity is in: `Milliliter`, `Gram` or `Piece`. It can't change
/// later.</param>
/// <param name="PackName">What it comes in, e.g. "keg", "bottle", "case".</param>
/// <param name="PackSize">How many base units one pack holds, e.g. 700 (ml) for a bottle.</param>
/// <param name="PackCost">What one pack costs, in euros.</param>
/// <param name="ParLevel">The quantity at or below which it counts as low, in base units.</param>
/// <param name="Status">The status. An inactive stock item is no longer bought.</param>
/// <param name="SupplierId">Who it is usually bought from, for the reorder list; optional.</param>
/// <param name="OpeningQuantity">What is on the shelf now, in base units; 0 starts it empty.</param>
internal sealed record CreateStockItemRequest(
    string Name,
    StockGroup Group,
    StockUnit Unit,
    string PackName,
    decimal PackSize,
    decimal PackCost,
    decimal ParLevel,
    StockItemStatus Status,
    Guid? SupplierId,
    decimal OpeningQuantity);

/// <summary>The created stock item.</summary>
/// <param name="Data">The stock item.</param>
internal sealed record CreateStockItemResponse(StockItemDto Data);

internal sealed class CreateStockItemEndpoint : IEndpoint
{
    // In base units: far above anything a bar stocks (a million ml is a thousand litres), below the columns' limits
    internal const decimal MaxQuantity = 1_000_000;

    internal const decimal MaxPackCost = 100_000;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPost("api/stock-items", HandleAsync)
            .WithName("CreateStockItem")
            .WithTags("Stock")
            .WithSummary("Create a stock item")
            .WithDescription("""
                Creates a stock item. An opening quantity above 0 is recorded as its first movement (`Opening`).

                - `404 Supplier.NotFound`: the supplier does not exist
                - `409 StockItem.NameAlreadyExists`: another stock item has this name
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<CreatedAtRoute<CreateStockItemResponse>, ProblemHttpResult>> HandleAsync(
        [FromBody] CreateStockItemRequest request,
        [FromServices] IValidator<CreateStockItemRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        CreateStockItemCommand command = new(
            request.Name,
            request.Group,
            request.Unit,
            request.PackName,
            request.PackSize,
            request.PackCost,
            request.ParLevel,
            request.Status,
            request.SupplierId,
            request.OpeningQuantity);

        Result<StockItemDto> result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(
                new CreateStockItemResponse(result.Value),
                "GetStockItem",
                new { id = result.Value.Id })
            : result.ToProblemDetails();
    }

    internal sealed class CreateStockItemRequestValidator : AbstractValidator<CreateStockItemRequest>
    {
        public CreateStockItemRequestValidator()
        {
            RuleFor(request => request.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name must be at most 100 characters");

            RuleFor(request => request.Group)
                .IsInEnum().WithMessage("Group is invalid");

            RuleFor(request => request.Unit)
                .IsInEnum().WithMessage("Unit is invalid");

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

            RuleFor(request => request.OpeningQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("OpeningQuantity must not be negative")
                .LessThanOrEqualTo(CreateStockItemEndpoint.MaxQuantity)
                .WithMessage($"OpeningQuantity must be at most {CreateStockItemEndpoint.MaxQuantity}");
        }
    }
}
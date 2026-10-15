using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Stock;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Stock;

/// <summary>What is really on the shelf.</summary>
/// <param name="Quantity">The actual quantity, in base units.</param>
/// <param name="Note">An optional reason, e.g. "Found a bottle in the cellar".</param>
internal sealed record CorrectStockRequest(decimal Quantity, string? Note);

internal sealed class CorrectStockEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPost("api/stock-items/{id:guid}/correction", HandleAsync)
            .WithName("CorrectStock")
            .WithTags("Stock")
            .WithSummary("Correct the quantity")
            .WithDescription("""
                Sets the stock item's quantity to what is really there, as a `Correction` movement of the difference.

                - `400 StockItem.QuantityUnchanged`: the quantity is already this
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromBody] CorrectStockRequest request,
        [FromServices] IValidator<CorrectStockRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        CorrectStockCommand command = new(id, request.Quantity, request.Note);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class CorrectStockRequestValidator : AbstractValidator<CorrectStockRequest>
    {
        public CorrectStockRequestValidator()
        {
            RuleFor(request => request.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("Quantity must not be negative")
                .LessThanOrEqualTo(CreateStockItemEndpoint.MaxQuantity)
                .WithMessage($"Quantity must be at most {CreateStockItemEndpoint.MaxQuantity}");

            RuleFor(request => request.Note)
                .MaximumLength(200).WithMessage("Note must be at most 200 characters");
        }
    }
}
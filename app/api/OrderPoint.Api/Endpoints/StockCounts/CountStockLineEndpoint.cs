using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Endpoints.Stock;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.StockCounts;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.StockCounts;

/// <summary>What was counted.</summary>
/// <param name="Quantity">The counted quantity, in base units; `null` clears the line.</param>
internal sealed record CountStockLineRequest(decimal? Quantity);

internal sealed class CountStockLineEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/stock-counts/{id:guid}/lines/{lineId:guid}", HandleAsync)
            .WithName("CountStockLine")
            .WithTags("Stock counts")
            .WithSummary("Enter a counted quantity")
            .WithDescription("""
                Records what was counted for one line, together with what the system expects right now and the unit
                cost, so sales after it still come off the stock when the count closes. `null` clears the line.

                - `404 StockCount.LineNotFound`: the line is not on this count
                - `409 StockCount.NotOpen`: the count is closed
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromRoute] Guid lineId,
        [FromBody] CountStockLineRequest request,
        [FromServices] IValidator<CountStockLineRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        Result result = await sender.Send(new CountStockLineCommand(id, lineId, request.Quantity), cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class CountStockLineRequestValidator : AbstractValidator<CountStockLineRequest>
    {
        public CountStockLineRequestValidator()
        {
            RuleFor(request => request.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("Quantity must not be negative")
                .LessThanOrEqualTo(CreateStockItemEndpoint.MaxQuantity)
                .WithMessage($"Quantity must be at most {CreateStockItemEndpoint.MaxQuantity}");
        }
    }
}
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Stock;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Stock;

/// <summary>What was lost.</summary>
/// <param name="Amount">How much was lost, in base units, e.g. 50 (ml).</param>
/// <param name="Note">An optional reason, e.g. "Spilled".</param>
internal sealed record RecordStockWasteRequest(decimal Amount, string? Note);

internal sealed class RecordStockWasteEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPost("api/stock-items/{id:guid}/waste", HandleAsync)
            .WithName("RecordStockWaste")
            .WithTags("Stock")
            .WithSummary("Record waste")
            .WithDescription("""
                Takes what was spilled, broken, expired or given away off the stock item, as a `Waste` movement.
                The quantity may go below 0. Items whose recipe it leaves short of one serving become sold out.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromBody] RecordStockWasteRequest request,
        [FromServices] IValidator<RecordStockWasteRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        RecordStockWasteCommand command = new(id, request.Amount, request.Note);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class RecordStockWasteRequestValidator : AbstractValidator<RecordStockWasteRequest>
    {
        public RecordStockWasteRequestValidator()
        {
            RuleFor(request => request.Amount)
                .GreaterThan(0).WithMessage("Amount must be positive")
                .LessThanOrEqualTo(CreateStockItemEndpoint.MaxQuantity)
                .WithMessage($"Amount must be at most {CreateStockItemEndpoint.MaxQuantity}");

            RuleFor(request => request.Note)
                .MaximumLength(200).WithMessage("Note must be at most 200 characters");
        }
    }
}
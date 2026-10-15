using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.StockCounts;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.StockCounts;

/// <summary>Closing a count.</summary>
/// <param name="Note">An optional note, e.g. "Month end"; the movements carry it (default "Stock count").</param>
internal sealed record CloseStockCountRequest(string? Note);

internal sealed class CloseStockCountEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/stock-counts/{id:guid}/close", HandleAsync)
            .WithName("CloseStockCount")
            .WithTags("Stock counts")
            .WithSummary("Close a stock count")
            .WithDescription("""
                Applies every counted line's difference (counted minus expected when it was counted) to its stock
                item as a `Count` movement, valued at the cost when it was counted; lines not counted stay as they
                are. Items whose recipe it leaves short of one serving become sold out, and sold-out ones come back.

                - `409 StockCount.NotOpen`: the count is already closed
                - `409 StockCount.NothingCounted`: no line was counted
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromBody] CloseStockCountRequest request,
        [FromServices] IValidator<CloseStockCountRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        Result result = await sender.Send(new CloseStockCountCommand(id, request.Note), cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class CloseStockCountRequestValidator : AbstractValidator<CloseStockCountRequest>
    {
        public CloseStockCountRequestValidator()
        {
            RuleFor(request => request.Note)
                .MaximumLength(200).WithMessage("Note must be at most 200 characters");
        }
    }
}
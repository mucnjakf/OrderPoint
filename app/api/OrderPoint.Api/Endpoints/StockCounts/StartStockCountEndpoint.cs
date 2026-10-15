using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.StockCounts;
using OrderPoint.Application.Dtos;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.StockCounts;

/// <summary>What to count.</summary>
/// <param name="Groups">The groups to count, each at most once.</param>
internal sealed record StartStockCountRequest(IReadOnlyList<StockGroup> Groups);

/// <summary>The started count.</summary>
/// <param name="Data">The count with a line per stock item.</param>
internal sealed record StartStockCountResponse(StockCountDto Data);

internal sealed class StartStockCountEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPost("api/stock-counts", HandleAsync)
            .WithName("StartStockCount")
            .WithTags("Stock counts")
            .WithSummary("Start a stock count")
            .WithDescription("""
                Starts a count with one line per active stock item in the chosen groups. Only one count can be open
                at a time.

                - `409 StockCount.AlreadyOpen`: another count is still open; close or discard it first
                - `409 StockCount.NoStockItems`: the groups have no active stock items
                """)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<CreatedAtRoute<StartStockCountResponse>, ProblemHttpResult>> HandleAsync(
        [FromBody] StartStockCountRequest request,
        [FromServices] IValidator<StartStockCountRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        Result<StockCountDto> result = await sender.Send(new StartStockCountCommand(request.Groups), cancellationToken);

        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(
                new StartStockCountResponse(result.Value),
                "GetStockCount",
                new { id = result.Value.Id })
            : result.ToProblemDetails();
    }

    internal sealed class StartStockCountRequestValidator : AbstractValidator<StartStockCountRequest>
    {
        public StartStockCountRequestValidator()
        {
            RuleFor(request => request.Groups)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Groups are required")
                .Must(groups => groups.Distinct().Count() == groups.Count)
                .WithMessage("Each group may be picked only once");

            RuleForEach(request => request.Groups)
                .IsInEnum().WithMessage("Group is invalid");
        }
    }
}
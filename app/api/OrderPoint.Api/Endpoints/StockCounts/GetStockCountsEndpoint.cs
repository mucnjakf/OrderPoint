using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.StockCounts;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Api.Endpoints.StockCounts;

/// <summary>Paging, search, filter and sort options.</summary>
/// <param name="PageNumber">The page to return, starting at 1.</param>
/// <param name="PageSize">The number of counts per page.</param>
/// <param name="SearchQuery">Matches the note, ignoring case.</param>
/// <param name="Status">Only open or only closed counts.</param>
/// <param name="SortBy">The sort order. Default: `ClosedAtUtcDesc` (the open count, then the latest closed).</param>
internal sealed record GetStockCountsRequest(
    [FromQuery] int PageNumber,
    [FromQuery] int PageSize,
    [FromQuery] string? SearchQuery,
    [FromQuery] StockCountStatus? Status,
    [FromQuery] StockCountSortBy? SortBy);

/// <summary>A page of stock counts.</summary>
/// <param name="Data">The page.</param>
internal sealed record GetStockCountsResponse(PaginationDto<StockCountDto> Data);

internal sealed class GetStockCountsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/stock-counts", HandleAsync)
            .WithName("GetStockCounts")
            .WithTags("Stock counts")
            .WithSummary("List stock counts")
            .WithDescription("""
                A page of stock counts with their lines, search (the note), a status filter and sorting (default: the
                open count, then the latest closed).
                """)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<GetStockCountsResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetStockCountsRequest request,
        [FromServices] IValidator<GetStockCountsRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetStockCountsQuery query = new(
            request.PageNumber,
            request.PageSize,
            request.SearchQuery,
            request.Status,
            request.SortBy);

        Result<PaginationDto<StockCountDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetStockCountsResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class GetStockCountsRequestValidator : AbstractValidator<GetStockCountsRequest>
    {
        public GetStockCountsRequestValidator()
        {
            RuleFor(request => request.PageNumber)
                .GreaterThan(0).WithMessage("PageNumber must be positive");

            RuleFor(request => request.PageSize)
                .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100");

            RuleFor(request => request.SearchQuery)
                .MaximumLength(100).WithMessage("SearchQuery must be at most 100 characters");

            RuleFor(request => request.Status)
                .IsInEnum().WithMessage("Status is invalid");

            RuleFor(request => request.SortBy)
                .IsInEnum().WithMessage("SortBy is invalid");
        }
    }
}
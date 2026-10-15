using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Stock;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Api.Endpoints.Stock;

/// <summary>Paging, search, filter and sort options.</summary>
/// <param name="PageNumber">The page to return, starting at 1.</param>
/// <param name="PageSize">The number of stock items per page.</param>
/// <param name="SearchQuery">Matches the name, ignoring case.</param>
/// <param name="Group">Only stock items of this group.</param>
/// <param name="Level">Only stock items at this level: `Out` (at or below 0), `Low` (above 0, at or below the par
/// level) or `Ok`.</param>
/// <param name="Status">Only stock items with this status.</param>
/// <param name="SortBy">The sort order within each group. Default: `NameAsc`.</param>
internal sealed record GetStockItemsRequest(
    [FromQuery] int PageNumber,
    [FromQuery] int PageSize,
    [FromQuery] string? SearchQuery,
    [FromQuery] StockGroup? Group,
    [FromQuery] StockLevel? Level,
    [FromQuery] StockItemStatus? Status,
    [FromQuery] StockItemSortBy? SortBy);

/// <summary>A page of stock items.</summary>
/// <param name="Data">The page.</param>
internal sealed record GetStockItemsResponse(PaginationDto<StockItemDto> Data);

internal sealed class GetStockItemsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/stock-items", HandleAsync)
            .WithName("GetStockItems")
            .WithTags("Stock")
            .WithSummary("List stock items")
            .WithDescription("""
                A page of stock items with search (name), group, level and status filters and sorting. The list is
                always sorted by group first (in the order of `StockGroup`), so a group's rows stay together across
                pages; `sortBy` orders the rows within each group (default: by name). `value` and `level` are
                calculated from the quantity, the pack cost and the par level.
                """)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<GetStockItemsResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetStockItemsRequest request,
        [FromServices] IValidator<GetStockItemsRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetStockItemsQuery query = new(
            request.PageNumber,
            request.PageSize,
            request.SearchQuery,
            request.Group,
            request.Level,
            request.Status,
            request.SortBy);

        Result<PaginationDto<StockItemDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetStockItemsResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class GetStockItemsRequestValidator : AbstractValidator<GetStockItemsRequest>
    {
        public GetStockItemsRequestValidator()
        {
            RuleFor(request => request.PageNumber)
                .GreaterThan(0).WithMessage("PageNumber must be positive");

            RuleFor(request => request.PageSize)
                .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100");

            RuleFor(request => request.SearchQuery)
                .MaximumLength(100).WithMessage("SearchQuery must be at most 100 characters");

            RuleFor(request => request.Group)
                .IsInEnum().WithMessage("Group is invalid");

            RuleFor(request => request.Level)
                .IsInEnum().WithMessage("Level is invalid");

            RuleFor(request => request.Status)
                .IsInEnum().WithMessage("Status is invalid");

            RuleFor(request => request.SortBy)
                .IsInEnum().WithMessage("SortBy is invalid");
        }
    }
}
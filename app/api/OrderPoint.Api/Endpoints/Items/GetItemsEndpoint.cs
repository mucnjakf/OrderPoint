using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Items;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Api.Endpoints.Items;

/// <summary>Paging, search, filter and sort options.</summary>
/// <param name="PageNumber">The page to return, starting at 1.</param>
/// <param name="PageSize">The number of items per page.</param>
/// <param name="SearchQuery">Matches the name, ignoring case.</param>
/// <param name="CategoryId">Only items of this category.</param>
/// <param name="Status">Only items with this effective status: an item in an inactive category counts as
/// inactive.</param>
/// <param name="SortBy">The sort order. Default: `CreatedAtUtcDesc` (newest first).</param>
internal sealed record GetItemsRequest(
    [FromQuery] int PageNumber,
    [FromQuery] int PageSize,
    [FromQuery] string? SearchQuery,
    [FromQuery] Guid? CategoryId,
    [FromQuery] ItemStatus? Status,
    [FromQuery] ItemSortBy? SortBy);

/// <summary>A page of items.</summary>
/// <param name="Data">The page.</param>
internal sealed record GetItemsResponse(PaginationDto<ItemDto> Data);

internal sealed class GetItemsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/items", HandleAsync)
            .WithName("GetItems")
            .WithTags("Items")
            .WithSummary("List items")
            .WithDescription("""
                A page of items with search (name), category and status filters and sorting (default: newest first).
                The status filter uses the effective status: an item in an inactive category counts as inactive.
                `ordersCount` is the number of orders that contain the item. The bar app reads it for its menu.
                """)
            .RequireAuthorization(AuthorizationPolicies.AdminOrBartender);
    }

    private static async Task<Results<Ok<GetItemsResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetItemsRequest request,
        [FromServices] IValidator<GetItemsRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetItemsQuery query = new(
            request.PageNumber,
            request.PageSize,
            request.SearchQuery,
            request.CategoryId,
            request.Status,
            request.SortBy);

        Result<PaginationDto<ItemDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetItemsResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class GetItemsRequestValidator : AbstractValidator<GetItemsRequest>
    {
        public GetItemsRequestValidator()
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
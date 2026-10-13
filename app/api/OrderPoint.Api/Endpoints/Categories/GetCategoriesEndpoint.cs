using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Categories;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Api.Endpoints.Categories;

/// <summary>Paging, search, filter and sort options.</summary>
/// <param name="PageNumber">The page to return, starting at 1.</param>
/// <param name="PageSize">The number of categories per page.</param>
/// <param name="SearchQuery">Matches the name, ignoring case.</param>
/// <param name="Status">Only categories with this status.</param>
/// <param name="SortBy">The sort order. Default: `CreatedAtUtcDesc` (newest first). `DisplayOrderAsc` gives the menu
/// order.</param>
internal sealed record GetCategoriesRequest(
    [FromQuery] int PageNumber,
    [FromQuery] int PageSize,
    [FromQuery] string? SearchQuery,
    [FromQuery] CategoryStatus? Status,
    [FromQuery] CategorySortBy? SortBy);

/// <summary>A page of categories.</summary>
/// <param name="Data">The page.</param>
internal sealed record GetCategoriesResponse(PaginationDto<CategoryDto> Data);

internal sealed class GetCategoriesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/categories", HandleAsync)
            .WithName("GetCategories")
            .WithTags("Categories")
            .WithSummary("List categories")
            .WithDescription("""
                A page of categories with search (name), a status filter and sorting (default: newest first;
                `DisplayOrderAsc` gives the menu order). `itemsCount` is the number of items in the category. The
                bartender app reads it for its menu.
                """)
            .RequireAuthorization(AuthorizationPolicies.AdminOrBartender);
    }

    private static async Task<Results<Ok<GetCategoriesResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetCategoriesRequest request,
        [FromServices] IValidator<GetCategoriesRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetCategoriesQuery query = new(
            request.PageNumber,
            request.PageSize,
            request.SearchQuery,
            request.Status,
            request.SortBy);

        Result<PaginationDto<CategoryDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetCategoriesResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class GetCategoriesRequestValidator : AbstractValidator<GetCategoriesRequest>
    {
        public GetCategoriesRequestValidator()
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
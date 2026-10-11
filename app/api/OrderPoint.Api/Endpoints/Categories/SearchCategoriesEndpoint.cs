using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Categories;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Categories;

/// <summary>The search text.</summary>
/// <param name="SearchQuery">Matches the name, ignoring case.</param>
internal sealed record SearchCategoriesRequest([FromQuery] string SearchQuery);

/// <summary>The matching categories.</summary>
/// <param name="Data">The categories, sorted by name.</param>
internal sealed record SearchCategoriesResponse(IReadOnlyList<CategoryDto> Data);

internal sealed class SearchCategoriesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/categories/search", HandleAsync)
            .WithName("SearchCategories")
            .WithTags("Categories")
            .WithSummary("Search categories")
            .WithDescription("""
                An unpaged lookup by name for autocompletes, sorted by name.
                """)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<SearchCategoriesResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] SearchCategoriesRequest request,
        [FromServices] IValidator<SearchCategoriesRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        SearchCategoriesQuery query = new(request.SearchQuery);

        Result<IReadOnlyList<CategoryDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new SearchCategoriesResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class SearchCategoriesRequestValidator : AbstractValidator<SearchCategoriesRequest>
    {
        public SearchCategoriesRequestValidator()
        {
            RuleFor(request => request.SearchQuery)
                .NotEmpty().WithMessage("SearchQuery is required")
                .MaximumLength(100).WithMessage("SearchQuery must be at most 100 characters");
        }
    }
}
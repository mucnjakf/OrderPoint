using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Categories;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Categories;

internal sealed record SearchCategoriesResponse(IReadOnlyList<CategoryDto> Data);

internal sealed class SearchCategoriesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/categories/{searchQuery:alpha}", HandleAsync)
            .WithName("SearchCategories")
            .WithTags("Categories");
    }

    private static async Task<Results<Ok<SearchCategoriesResponse>, ProblemHttpResult>> HandleAsync(
        [FromRoute] string searchQuery,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        SearchCategoriesQuery query = new(searchQuery);

        Result<IReadOnlyList<CategoryDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new SearchCategoriesResponse(result.Value))
            : result.ToProblemDetails();
    }
}
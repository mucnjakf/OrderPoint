using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Categories;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Categories;

/// <summary>One category.</summary>
/// <param name="Data">The category.</param>
internal sealed record GetCategoryResponse(CategoryDto Data);

internal sealed class GetCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/categories/{id:guid}", HandleAsync)
            .WithName("GetCategory")
            .WithTags("Categories")
            .WithSummary("Get a category")
            .WithDescription("""
                Returns one category, including its number of items. The bar app reads it for its menu.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthorizationPolicies.AdminOrBartender);
    }

    private static async Task<Results<Ok<GetCategoryResponse>, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        GetCategoryQuery query = new(id);

        Result<CategoryDto> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetCategoryResponse(result.Value))
            : result.ToProblemDetails();
    }
}
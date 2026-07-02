using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Categories;

internal sealed record GetCategoryResponse(CategoryDto Data);

internal sealed class GetCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/categories/{id:guid}", HandleAsync)
            .WithName("GetCategory")
            .WithTags("Categories");
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
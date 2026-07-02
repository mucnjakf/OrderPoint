using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Api.Endpoints.Categories;

internal sealed record GetCategoriesResponse(PaginationDto<CategoryDto> Data);

internal sealed class GetCategoriesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/categories", HandleAsync)
            .WithName("GetCategories")
            .WithTags("Categories");
    }

    private static async Task<Results<Ok<GetCategoriesResponse>, ProblemHttpResult>> HandleAsync(
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        [FromQuery] string? searchQuery,
        [FromQuery] CategoryStatus? status,
        [FromQuery] CategorySortBy? sortBy,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        GetCategoriesQuery query = new(pageNumber, pageSize, searchQuery, status, sortBy);

        Result<PaginationDto<CategoryDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetCategoriesResponse(result.Value))
            : result.ToProblemDetails();
    }
}
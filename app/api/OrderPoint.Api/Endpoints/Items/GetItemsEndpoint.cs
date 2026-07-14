using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Items;
using OrderPoint.Domain.Outcomes;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Api.Endpoints.Items;

internal sealed record GetItemsResponse(PaginationDto<ItemDto> Data);

internal sealed class GetItemsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/items", HandleAsync)
            .WithName("GetItems")
            .WithTags("Items");
    }

    private static async Task<Results<Ok<GetItemsResponse>, ProblemHttpResult>> HandleAsync(
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        [FromQuery] string? searchQuery,
        [FromQuery] Guid? categoryId,
        [FromQuery] ItemSortBy? sortBy,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        GetItemsQuery query = new(pageNumber, pageSize, searchQuery, categoryId, sortBy);

        Result<PaginationDto<ItemDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetItemsResponse(result.Value))
            : result.ToProblemDetails();
    }
}
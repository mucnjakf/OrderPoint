using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Items;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Items;

/// <summary>One item.</summary>
/// <param name="Data">The item.</param>
internal sealed record GetItemResponse(ItemDto Data);

internal sealed class GetItemEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/items/{id:guid}", HandleAsync)
            .WithName("GetItem")
            .WithTags("Items")
            .WithSummary("Get an item")
            .WithDescription("""
                Returns one item with its category and the number of orders that contain it. The bar app reads
                it for its menu.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthorizationPolicies.AdminOrBartender);
    }

    private static async Task<Results<Ok<GetItemResponse>, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        GetItemQuery query = new(id);

        Result<ItemDto> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetItemResponse(result.Value))
            : result.ToProblemDetails();
    }
}
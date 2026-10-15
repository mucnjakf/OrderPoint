using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Items;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Items;

/// <summary>A menu item's recipe.</summary>
/// <param name="Data">The ingredients, the cost of one serving and the margin.</param>
internal sealed record GetItemRecipeResponse(ItemRecipeDto Data);

internal sealed class GetItemRecipeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/items/{id:guid}/recipe", HandleAsync)
            .WithName("GetItemRecipe")
            .WithTags("Items")
            .WithSummary("Get an item's recipe")
            .WithDescription("""
                The stock one serving uses, with what it costs at the stock items' current pack costs and the margin
                at the item's price. Empty for an item whose stock isn't tracked.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<GetItemRecipeResponse>, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        GetItemRecipeQuery query = new(id);

        Result<ItemRecipeDto> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetItemRecipeResponse(result.Value))
            : result.ToProblemDetails();
    }
}
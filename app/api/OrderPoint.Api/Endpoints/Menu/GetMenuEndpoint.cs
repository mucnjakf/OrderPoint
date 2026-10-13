using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Menu;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Menu;

/// <summary>The guests' menu.</summary>
/// <param name="Data">The categories with their items.</param>
internal sealed record GetMenuResponse(IReadOnlyList<MenuCategoryDto> Data);

internal sealed class GetMenuEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/menu", HandleAsync)
            .WithName("GetMenu")
            .WithTags("Menu")
            .WithSummary("Get the menu")
            .WithDescription("""
                The menu as guests see it in the customer app, so no token is needed: the active categories in menu
                order (`displayOrder`), each with its active items by name. Categories without active items are left
                out, and so is everything only staff needs (statuses, order counts). Cached; any category or item
                change clears it.
                """);
    }

    private static async Task<Results<Ok<GetMenuResponse>, ProblemHttpResult>> HandleAsync(
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        GetMenuQuery query = new();

        Result<IReadOnlyList<MenuCategoryDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetMenuResponse(result.Value))
            : result.ToProblemDetails();
    }
}
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Categories;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Categories;

internal sealed class DeleteCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapDelete("api/categories/{id:guid}", HandleAsync)
            .WithName("DeleteCategory")
            .WithTags("Categories")
            .WithSummary("Delete a category")
            .WithDescription("""
                Deletes the category and its image.

                - `409 Category.CannotDeleteCategoryWithItems`: the category still has items; move or delete them
                  first.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        DeleteCategoryCommand command = new(id);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }
}
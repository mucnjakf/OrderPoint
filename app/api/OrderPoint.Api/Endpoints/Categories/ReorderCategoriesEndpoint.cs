using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Categories;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Categories;

/// <summary>The new menu order.</summary>
/// <param name="Ids">The id of every category exactly once, in the new menu order (first on the menu first).</param>
internal sealed record ReorderCategoriesRequest(IReadOnlyList<Guid> Ids);

internal sealed class ReorderCategoriesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/categories/display-order", HandleAsync)
            .WithName("ReorderCategories")
            .WithTags("Categories")
            .WithSummary("Reorder categories")
            .WithDescription("""
                Sets the menu order: send the id of every category exactly once, in the new order. The categories are
                numbered 1 to n.

                - `400 Category.DisplayOrderMustIncludeAllCategories`: a category is missing or listed twice.
                - `404 Category.NotFound`: an id does not exist.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromBody] ReorderCategoriesRequest request,
        [FromServices] IValidator<ReorderCategoriesRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        ReorderCategoriesCommand command = new(request.Ids);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class ReorderCategoriesRequestValidator : AbstractValidator<ReorderCategoriesRequest>
    {
        public ReorderCategoriesRequestValidator()
        {
            RuleFor(request => request.Ids)
                .MustBeValidIds();
        }
    }
}
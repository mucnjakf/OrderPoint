using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Categories;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Categories;

/// <summary>The categories to delete.</summary>
/// <param name="Ids">The ids of 1 to 100 different categories.</param>
internal sealed record DeleteCategoriesRequest(IReadOnlyList<Guid> Ids);

internal sealed class DeleteCategoriesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPost("api/categories/delete", HandleAsync)
            .WithName("DeleteCategories")
            .WithTags("Categories")
            .WithSummary("Delete categories")
            .WithDescription("""
                Deletes up to 100 categories with their images. All-or-nothing: if any id does not exist (`404`),
                nothing changes.

                - `409 Category.CannotDeleteCategoryWithItems`: one of them still has items; nothing is deleted.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromBody] DeleteCategoriesRequest request,
        [FromServices] IValidator<DeleteCategoriesRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        DeleteCategoriesCommand command = new(request.Ids);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class DeleteCategoriesRequestValidator : AbstractValidator<DeleteCategoriesRequest>
    {
        public DeleteCategoriesRequestValidator()
        {
            RuleFor(request => request.Ids)
                .MustBeValidIds();
        }
    }
}
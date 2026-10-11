using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Items;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Items;

/// <summary>The items to delete.</summary>
/// <param name="Ids">The ids of 1 to 100 different items.</param>
internal sealed record DeleteItemsRequest(IReadOnlyList<Guid> Ids);

internal sealed class DeleteItemsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPost("api/items/delete", HandleAsync)
            .WithName("DeleteItems")
            .WithTags("Items")
            .WithSummary("Delete items")
            .WithDescription("""
                Deletes up to 100 items with their images. All-or-nothing: if any id does not exist (`404`), nothing
                changes.

                - `409 Item.CannotDeleteItemWithOrders`: one of them is part of orders; nothing is deleted.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromBody] DeleteItemsRequest request,
        [FromServices] IValidator<DeleteItemsRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        DeleteItemsCommand command = new(request.Ids);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class DeleteItemsRequestValidator : AbstractValidator<DeleteItemsRequest>
    {
        public DeleteItemsRequestValidator()
        {
            RuleFor(request => request.Ids)
                .MustBeValidIds();
        }
    }
}
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Items;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Items;

/// <summary>The items and their new status.</summary>
/// <param name="Ids">The ids of 1 to 100 different items.</param>
/// <param name="Status">The new status for all of them. Inactive items cannot be ordered.</param>
internal sealed record ChangeItemsStatusRequest(IReadOnlyList<Guid> Ids, ItemStatus Status);

internal sealed class ChangeItemsStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/items/status", HandleAsync)
            .WithName("ChangeItemsStatus")
            .WithTags("Items")
            .WithSummary("Change the status of items")
            .WithDescription("""
                Activates or deactivates up to 100 items at once. An active item in an inactive category still cannot
                be ordered. All-or-nothing: if any id does not exist (`404`), nothing changes.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromBody] ChangeItemsStatusRequest request,
        [FromServices] IValidator<ChangeItemsStatusRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        ChangeItemsStatusCommand command = new(request.Ids, request.Status);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class ChangeItemsStatusRequestValidator : AbstractValidator<ChangeItemsStatusRequest>
    {
        public ChangeItemsStatusRequestValidator()
        {
            RuleFor(request => request.Ids)
                .MustBeValidIds();

            RuleFor(request => request.Status)
                .IsInEnum().WithMessage("Status is invalid");
        }
    }
}
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Categories;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Categories;

/// <summary>The categories and their new status.</summary>
/// <param name="Ids">The ids of 1 to 100 different categories.</param>
/// <param name="Status">The new status for all of them. Items in an inactive category cannot be ordered.</param>
internal sealed record ChangeCategoriesStatusRequest(IReadOnlyList<Guid> Ids, CategoryStatus Status);

internal sealed class ChangeCategoriesStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/categories/status", HandleAsync)
            .WithName("ChangeCategoriesStatus")
            .WithTags("Categories")
            .WithSummary("Change the status of categories")
            .WithDescription("""
                Activates or deactivates up to 100 categories at once. Their items keep their own status.
                All-or-nothing: if any id does not exist (`404`), nothing changes. Bartenders use it to take a whole
                category off the menu.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthorizationPolicies.AdminOrBartender);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromBody] ChangeCategoriesStatusRequest request,
        [FromServices] IValidator<ChangeCategoriesStatusRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        ChangeCategoriesStatusCommand command = new(request.Ids, request.Status);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class ChangeCategoriesStatusRequestValidator : AbstractValidator<ChangeCategoriesStatusRequest>
    {
        public ChangeCategoriesStatusRequestValidator()
        {
            RuleFor(request => request.Ids)
                .MustBeValidIds();

            RuleFor(request => request.Status)
                .IsInEnum().WithMessage("Status is invalid");
        }
    }
}
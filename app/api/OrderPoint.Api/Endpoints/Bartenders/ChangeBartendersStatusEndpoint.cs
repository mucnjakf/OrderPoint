using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Bartenders;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Bartenders;

/// <summary>The bartenders and their new status.</summary>
/// <param name="Ids">The ids of 1 to 100 different bartenders.</param>
/// <param name="Status">The new status for all of them. Inactive bartenders cannot sign in.</param>
internal sealed record ChangeBartendersStatusRequest(IReadOnlyList<Guid> Ids, BartenderStatus Status);

internal sealed class ChangeBartendersStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/bartenders/status", HandleAsync)
            .WithName("ChangeBartendersStatus")
            .WithTags("Bartenders")
            .WithSummary("Change the status of bartenders")
            .WithDescription("""
                Activates or deactivates up to 100 bartenders at once. Deactivated bartenders are signed out, and
                their accepted and active orders go back in the queue (`Pending`, without bartender).
                All-or-nothing: if any id does not exist (`404`), nothing changes.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromBody] ChangeBartendersStatusRequest request,
        [FromServices] IValidator<ChangeBartendersStatusRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        ChangeBartendersStatusCommand command = new(request.Ids, request.Status);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class ChangeBartendersStatusRequestValidator : AbstractValidator<ChangeBartendersStatusRequest>
    {
        public ChangeBartendersStatusRequestValidator()
        {
            RuleFor(request => request.Ids)
                .MustBeValidIds();

            RuleFor(request => request.Status)
                .IsInEnum().WithMessage("Status is invalid");
        }
    }
}
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Bartenders;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Bartenders;

/// <summary>The bartenders to delete.</summary>
/// <param name="Ids">The ids of 1 to 100 different bartenders.</param>
internal sealed record DeleteBartendersRequest(IReadOnlyList<Guid> Ids);

internal sealed class DeleteBartendersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPost("api/bartenders/delete", HandleAsync)
            .WithName("DeleteBartenders")
            .WithTags("Bartenders")
            .WithSummary("Delete bartenders")
            .WithDescription("""
                Deletes up to 100 bartenders with their logins and images. All-or-nothing: if any id does not exist
                (`404`), nothing changes.

                - `409 Bartender.CannotDeleteBartenderWithOrders`: one of them handled orders; nothing is deleted.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromBody] DeleteBartendersRequest request,
        [FromServices] IValidator<DeleteBartendersRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        DeleteBartendersCommand command = new(request.Ids);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class DeleteBartendersRequestValidator : AbstractValidator<DeleteBartendersRequest>
    {
        public DeleteBartendersRequestValidator()
        {
            RuleFor(request => request.Ids)
                .MustBeValidIds();
        }
    }
}
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.TableRequests;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.TableRequests;

internal sealed class CompleteTableRequestEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/table-requests/{id:guid}/done", HandleAsync)
            .WithName("CompleteTableRequest")
            .WithTags("Table requests")
            .WithSummary("Mark a request done")
            .WithDescription("""
                The signed-in bartender went to the table or brought the bill; the request disappears from the
                bar app and from the guest's page.

                - `403 Auth.AccountInactive`: the signed-in bartender was deactivated
                - `403 Auth.AccountDeleted`: the signed-in bartender no longer exists
                - `404 TableRequest.NotFound`: the request does not exist
                - `409 TableRequest.AlreadyDone`: another bartender marked it done first
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthorizationPolicies.Bartender);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        CompleteTableRequestCommand command = new(id, user.GetUserId());

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }
}
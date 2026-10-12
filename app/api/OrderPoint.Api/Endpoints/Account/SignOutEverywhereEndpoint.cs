using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Account;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Account;

internal sealed class SignOutEverywhereEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPost("api/account/sign-out-everywhere", HandleAsync)
            .WithName("SignOutEverywhere")
            .WithTags("Account")
            .WithSummary("Sign out everywhere")
            .WithDescription("""
                Revokes every refresh token of the signed-in user, including the one of this session. Access tokens
                already issued stay valid until they expire (15 minutes).
                """)
            .RequireAuthorization(AuthorizationPolicies.AdminOrBartender);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromServices] ISender sender,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        SignOutEverywhereCommand command = new(user.GetUserId());

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }
}
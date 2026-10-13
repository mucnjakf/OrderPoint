using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Account;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Account;

/// <summary>The signed-in user's account.</summary>
/// <param name="Data">The account.</param>
internal sealed record GetAccountResponse(AccountDto Data);

internal sealed class GetAccountEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/account", HandleAsync)
            .WithName("GetAccount")
            .WithTags("Account")
            .WithSummary("Get my account")
            .WithDescription("""
                Returns the signed-in user's email and profile image: the admin's own image, or for a bartender the
                photo the admin set. For a bartender it also returns the name and phone number the admin set. The
                user is taken from the access token.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthorizationPolicies.AdminOrBartender);
    }

    private static async Task<Results<Ok<GetAccountResponse>, ProblemHttpResult>> HandleAsync(
        [FromServices] ISender sender,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        GetAccountQuery query = new(user.GetUserId());

        Result<AccountDto> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetAccountResponse(result.Value))
            : result.ToProblemDetails();
    }
}
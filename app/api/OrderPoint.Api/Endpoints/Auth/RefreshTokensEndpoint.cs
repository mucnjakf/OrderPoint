using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Auth;
using OrderPoint.Application.Dtos;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Auth;

/// <summary>The refresh token to exchange.</summary>
/// <param name="RefreshToken">The refresh token from signing in or from the last refresh. It works once.</param>
internal sealed record RefreshTokensRequest(string RefreshToken);

/// <summary>The new token pair.</summary>
/// <param name="Data">The new tokens; the old refresh token no longer works.</param>
internal sealed record RefreshTokensResponse(AuthTokensDto Data);

internal sealed class RefreshTokensEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPost("api/auth/refresh", HandleAsync)
            .WithName("RefreshTokens")
            .WithTags("Auth")
            .WithSummary("Refresh the tokens")
            .WithDescription("""
                Exchanges a refresh token for a new access token and refresh token. The used refresh token is revoked
                (rotation), so each one works once.

                - `401 Auth.InvalidRefreshToken`: the token is unknown, expired, revoked or already used.
                """)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<Results<Ok<RefreshTokensResponse>, ProblemHttpResult>> HandleAsync(
        [FromBody] RefreshTokensRequest request,
        [FromServices] IValidator<RefreshTokensRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        RefreshTokensCommand command = new(request.RefreshToken);

        Result<AuthTokensDto> result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new RefreshTokensResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class RefreshTokensRequestValidator : AbstractValidator<RefreshTokensRequest>
    {
        public RefreshTokensRequestValidator()
        {
            RuleFor(request => request.RefreshToken)
                .NotEmpty().WithMessage("RefreshToken is required")
                .MaximumLength(200).WithMessage("RefreshToken must be at most 200 characters");
        }
    }
}
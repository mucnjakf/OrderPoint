using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Auth;
using OrderPoint.Application.Dtos;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Auth;

/// <summary>The credentials and the role of the app signing in.</summary>
/// <param name="Email">The email the user signs in with.</param>
/// <param name="Password">The password.</param>
/// <param name="Role">The role of the app: `0` = Admin (Admin web), `1` = Bartender (bar app). A user of the
/// other role gets `401 Auth.InvalidCredentials`.</param>
internal sealed record LoginRequest(string Email, string Password, UserRole Role);

/// <summary>The tokens of the new session.</summary>
/// <param name="Data">The tokens.</param>
internal sealed record LoginResponse(AuthTokensDto Data);

internal sealed class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPost("api/auth/login", HandleAsync)
            .WithName("Login")
            .WithTags("Auth")
            .WithSummary("Sign in")
            .WithDescription("""
                Checks the email, password and role and returns an access token and a refresh token.

                - `401 Auth.InvalidCredentials`: wrong email or password, or the user has the other role (the same
                  error on purpose).
                - `401 Auth.LockedOut`: 5 failed attempts in a row; the account is locked for 5 minutes.
                - `403 Auth.PasswordChangeRequired`: a bartender with a temporary password; use `POST
                  /api/auth/initial-password`.
                - `403 Auth.AccountInactive`: the bartender is inactive.
                """)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    private static async Task<Results<Ok<LoginResponse>, ProblemHttpResult>> HandleAsync(
        [FromBody] LoginRequest request,
        [FromServices] IValidator<LoginRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        LoginCommand command = new(request.Email, request.Password, request.Role);

        Result<AuthTokensDto> result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new LoginResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(request => request.Email)
                .NotEmpty().WithMessage("Email is required")
                .MaximumLength(100).WithMessage("Email must be at most 100 characters");

            RuleFor(request => request.Password)
                .NotEmpty().WithMessage("Password is required")
                .MaximumLength(100).WithMessage("Password must be at most 100 characters");

            RuleFor(request => request.Role)
                .IsInEnum().WithMessage("Role is invalid");
        }
    }
}
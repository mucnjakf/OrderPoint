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

/// <summary>The credentials with the temporary password and the new password.</summary>
/// <param name="Email">The email the bartender signs in with.</param>
/// <param name="CurrentPassword">The temporary password set by the admin.</param>
/// <param name="NewPassword">The new password. At least 8 characters with an uppercase letter, a lowercase letter and
/// a digit. It must differ from the temporary one.</param>
/// <param name="Role">The role of the app: the bar app sends `1` (Bartender).</param>
internal sealed record ChangeInitialPasswordRequest(
    string Email,
    string CurrentPassword,
    string NewPassword,
    UserRole Role);

/// <summary>The tokens after the first password was set.</summary>
/// <param name="Data">The tokens.</param>
internal sealed record ChangeInitialPasswordResponse(AuthTokensDto Data);

internal sealed class ChangeInitialPasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPost("api/auth/initial-password", HandleAsync)
            .WithName("ChangeInitialPassword")
            .WithTags("Auth")
            .WithSummary("Set the first password")
            .WithDescription("""
                First sign-in of a bartender created by the admin, or after the admin reset their password: replaces
                the temporary password and returns tokens. Works only while a password change is required.

                - `401 Auth.InvalidCredentials` or `401 Auth.LockedOut`: as for sign in.
                - `403 Auth.PasswordChangeNotAllowed`: the password was already changed.
                - `403 Auth.AccountInactive`: the bartender is inactive.
                - `400 Auth.InvalidPassword`: the new password needs 8+ characters, upper and lower case and a digit.
                """)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    private static async Task<Results<Ok<ChangeInitialPasswordResponse>, ProblemHttpResult>> HandleAsync(
        [FromBody] ChangeInitialPasswordRequest request,
        [FromServices] IValidator<ChangeInitialPasswordRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        ChangeInitialPasswordCommand command = new(
            request.Email,
            request.CurrentPassword,
            request.NewPassword,
            request.Role);

        Result<AuthTokensDto> result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new ChangeInitialPasswordResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class ChangeInitialPasswordRequestValidator : AbstractValidator<ChangeInitialPasswordRequest>
    {
        public ChangeInitialPasswordRequestValidator()
        {
            RuleFor(request => request.Email)
                .NotEmpty().WithMessage("Email is required")
                .MaximumLength(100).WithMessage("Email must be at most 100 characters");

            RuleFor(request => request.CurrentPassword)
                .NotEmpty().WithMessage("CurrentPassword is required")
                .MaximumLength(100).WithMessage("CurrentPassword must be at most 100 characters");

            RuleFor(request => request.NewPassword)
                .NotEmpty().WithMessage("NewPassword is required")
                .MinimumLength(8).WithMessage("NewPassword must be at least 8 characters")
                .MaximumLength(100).WithMessage("NewPassword must be at most 100 characters")
                .NotEqual(request => request.CurrentPassword)
                .WithMessage("NewPassword must be different from CurrentPassword");

            RuleFor(request => request.Role)
                .IsInEnum().WithMessage("Role is invalid");
        }
    }
}
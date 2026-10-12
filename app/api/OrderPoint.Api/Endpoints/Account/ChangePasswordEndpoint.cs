using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Account;
using OrderPoint.Application.Dtos;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Account;

/// <summary>The current and the new password.</summary>
/// <param name="CurrentPassword">The password the user signs in with now.</param>
/// <param name="NewPassword">The new password. At least 8 characters with an uppercase letter, a lowercase letter and
/// a digit. It must differ from the current one.</param>
internal sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

/// <summary>The new token pair for this session.</summary>
/// <param name="Data">The new tokens.</param>
internal sealed record ChangePasswordResponse(AuthTokensDto Data);

internal sealed class ChangePasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/account/password", HandleAsync)
            .WithName("ChangePassword")
            .WithTags("Account")
            .WithSummary("Change my password")
            .WithDescription("""
                Changes the signed-in user's password. Every refresh token is revoked, so other devices are signed
                out; the response contains a new token pair for this session.

                - `400 Auth.InvalidCurrentPassword`: the current password is wrong.
                - `400 Auth.InvalidPassword`: the new password needs 8+ characters, upper and lower case and a digit.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthorizationPolicies.AdminOrBartender);
    }

    private static async Task<Results<Ok<ChangePasswordResponse>, ProblemHttpResult>> HandleAsync(
        [FromBody] ChangePasswordRequest request,
        [FromServices] IValidator<ChangePasswordRequest> validator,
        [FromServices] ISender sender,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        ChangePasswordCommand command = new(user.GetUserId(), request.CurrentPassword, request.NewPassword);

        Result<AuthTokensDto> result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new ChangePasswordResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordRequestValidator()
        {
            RuleFor(request => request.CurrentPassword)
                .NotEmpty().WithMessage("CurrentPassword is required")
                .MaximumLength(100).WithMessage("CurrentPassword must be at most 100 characters");

            RuleFor(request => request.NewPassword)
                .NotEmpty().WithMessage("NewPassword is required")
                .MinimumLength(8).WithMessage("NewPassword must be at least 8 characters")
                .MaximumLength(100).WithMessage("NewPassword must be at most 100 characters")
                .NotEqual(request => request.CurrentPassword)
                .WithMessage("NewPassword must be different from CurrentPassword");
        }
    }
}
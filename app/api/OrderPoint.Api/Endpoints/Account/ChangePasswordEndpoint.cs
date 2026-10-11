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

internal sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

internal sealed record ChangePasswordResponse(AuthTokensDto Data);

internal sealed class ChangePasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/account/password", HandleAsync)
            .WithName("ChangePassword")
            .WithTags("Account")
            .RequireAuthorization(AuthorizationPolicies.Admin);
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
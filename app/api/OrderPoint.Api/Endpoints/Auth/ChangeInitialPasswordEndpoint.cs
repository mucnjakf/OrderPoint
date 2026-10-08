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

internal sealed record ChangeInitialPasswordRequest(
    string Email,
    string CurrentPassword,
    string NewPassword,
    UserRole Role);

internal sealed record ChangeInitialPasswordResponse(AuthTokensDto Data);

internal sealed class ChangeInitialPasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPost("api/auth/initial-password", HandleAsync)
            .WithName("ChangeInitialPassword")
            .WithTags("Auth");
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
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Bartenders;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Bartenders;

/// <summary>The new temporary password.</summary>
/// <param name="Password">The temporary password the bartender replaces at the next sign-in. At least 8 characters
/// with an uppercase letter, a lowercase letter and a digit.</param>
internal sealed record ResetBartenderPasswordRequest(string Password);

internal sealed class ResetBartenderPasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/bartenders/{id:guid}/password", HandleAsync)
            .WithName("ResetBartenderPassword")
            .WithTags("Bartenders")
            .WithSummary("Reset a bartender's password")
            .WithDescription("""
                Gives a bartender who forgot their password a new temporary one: they must replace it at the next
                sign-in, a lockout is lifted and every session is signed out.

                - `400 Auth.InvalidPassword`: the password needs 8+ characters, upper and lower case and a digit.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromBody] ResetBartenderPasswordRequest request,
        [FromServices] IValidator<ResetBartenderPasswordRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        ResetBartenderPasswordCommand command = new(id, request.Password);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class ResetBartenderPasswordRequestValidator : AbstractValidator<ResetBartenderPasswordRequest>
    {
        public ResetBartenderPasswordRequestValidator()
        {
            RuleFor(request => request.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .MaximumLength(100).WithMessage("Password must be at most 100 characters");
        }
    }
}
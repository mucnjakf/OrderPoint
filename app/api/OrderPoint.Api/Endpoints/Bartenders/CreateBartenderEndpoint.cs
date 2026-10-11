using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Bartenders;
using OrderPoint.Application.Dtos;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Bartenders;

/// <summary>The new bartender and their temporary password.</summary>
/// <param name="FirstName">The first name.</param>
/// <param name="LastName">The last name.</param>
/// <param name="Email">The email the bartender signs in with. It must be unique.</param>
/// <param name="Password">The temporary password the bartender replaces at the first sign-in. At least 8 characters
/// with an uppercase letter, a lowercase letter and a digit.</param>
/// <param name="PhoneNumber">An optional phone number.</param>
/// <param name="Status">The status. Inactive bartenders cannot sign in.</param>
/// <param name="Notes">Optional internal notes about the bartender.</param>
internal sealed record CreateBartenderRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? PhoneNumber,
    BartenderStatus Status,
    string? Notes);

/// <summary>The created bartender.</summary>
/// <param name="Data">The bartender.</param>
internal sealed record CreateBartenderResponse(BartenderDto Data);

internal sealed class CreateBartenderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPost("api/bartenders", HandleAsync)
            .WithName("CreateBartender")
            .WithTags("Bartenders")
            .WithSummary("Create a bartender")
            .WithDescription("""
                Creates the bartender and their login. `password` is temporary: the bartender has to replace it at the
                first sign-in (`POST /api/auth/initial-password`).

                - `409 Bartender.EmailAlreadyExists`: the email is already used.
                - `400 Auth.InvalidPassword`: the password needs 8+ characters, upper and lower case and a digit.
                """)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<CreatedAtRoute<CreateBartenderResponse>, ProblemHttpResult>> HandleAsync(
        [FromBody] CreateBartenderRequest request,
        [FromServices] IValidator<CreateBartenderRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        CreateBartenderCommand command = new(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password,
            request.PhoneNumber,
            request.Status,
            request.Notes);

        Result<BartenderDto> result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(
                new CreateBartenderResponse(result.Value),
                "GetBartender",
                new { id = result.Value.Id })
            : result.ToProblemDetails();
    }

    internal sealed class CreateBartenderRequestValidator : AbstractValidator<CreateBartenderRequest>
    {
        public CreateBartenderRequestValidator()
        {
            RuleFor(request => request.FirstName)
                .NotEmpty().WithMessage("FirstName is required")
                .MaximumLength(30).WithMessage("FirstName must be at most 30 characters");

            RuleFor(request => request.LastName)
                .NotEmpty().WithMessage("LastName is required")
                .MaximumLength(30).WithMessage("LastName must be at most 30 characters");

            RuleFor(request => request.Email)
                .NotEmpty().WithMessage("Email is required")
                .MaximumLength(100).WithMessage("Email must be at most 100 characters")
                .EmailAddress().WithMessage("Email is invalid");

            RuleFor(request => request.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .MaximumLength(100).WithMessage("Password must be at most 100 characters");

            RuleFor(request => request.PhoneNumber)
                .MaximumLength(20).WithMessage("PhoneNumber must be at most 20 characters");

            RuleFor(request => request.Status)
                .IsInEnum().WithMessage("Status is invalid");

            RuleFor(request => request.Notes)
                .MaximumLength(500).WithMessage("Notes must be at most 500 characters");
        }
    }
}
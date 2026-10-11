using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Bartenders;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Bartenders;

/// <summary>The bartender's new profile.</summary>
/// <param name="FirstName">The first name.</param>
/// <param name="LastName">The last name.</param>
/// <param name="Email">The email the bartender signs in with. It must be unique; changing it changes the sign-in
/// email.</param>
/// <param name="PhoneNumber">An optional phone number.</param>
/// <param name="Status">The status. Deactivating signs the bartender out.</param>
/// <param name="Notes">Optional internal notes about the bartender.</param>
internal sealed record UpdateBartenderRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    BartenderStatus Status,
    string? Notes);

internal sealed class UpdateBartenderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/bartenders/{id:guid}", HandleAsync)
            .WithName("UpdateBartender")
            .WithTags("Bartenders")
            .WithSummary("Update a bartender")
            .WithDescription("""
                Updates the profile. A new email also changes the login email; deactivating signs the bartender out
                (their refresh tokens are revoked).

                - `409 Bartender.EmailAlreadyExists`: the new email is already used.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateBartenderRequest request,
        [FromServices] IValidator<UpdateBartenderRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        UpdateBartenderCommand command = new(
            id,
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.Status,
            request.Notes);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class UpdateBartenderRequestValidator : AbstractValidator<UpdateBartenderRequest>
    {
        public UpdateBartenderRequestValidator()
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

            RuleFor(request => request.PhoneNumber)
                .MaximumLength(20).WithMessage("PhoneNumber must be at most 20 characters");

            RuleFor(request => request.Status)
                .IsInEnum().WithMessage("Status is invalid");

            RuleFor(request => request.Notes)
                .MaximumLength(500).WithMessage("Notes must be at most 500 characters");
        }
    }
}
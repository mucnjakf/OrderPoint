using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Suppliers;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Suppliers;

/// <summary>The supplier's new values.</summary>
/// <param name="Name">The name, e.g. "Metro"; unique.</param>
/// <param name="ContactName">Who to talk to; optional.</param>
/// <param name="Phone">The phone number; optional.</param>
/// <param name="Email">The email address; optional.</param>
/// <param name="Note">A note, e.g. delivery days; optional.</param>
/// <param name="Status">The status. An inactive supplier is no longer bought from.</param>
internal sealed record UpdateSupplierRequest(
    string Name,
    string? ContactName,
    string? Phone,
    string? Email,
    string? Note,
    SupplierStatus Status);

internal sealed class UpdateSupplierEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/suppliers/{id:guid}", HandleAsync)
            .WithName("UpdateSupplier")
            .WithTags("Suppliers")
            .WithSummary("Update a supplier")
            .WithDescription("""
                Updates the supplier's name, contact details, note and status.

                - `409 Supplier.NameAlreadyExists`: another supplier has this name
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateSupplierRequest request,
        [FromServices] IValidator<UpdateSupplierRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        UpdateSupplierCommand command = new(
            id,
            request.Name,
            request.ContactName,
            request.Phone,
            request.Email,
            request.Note,
            request.Status);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class UpdateSupplierRequestValidator : AbstractValidator<UpdateSupplierRequest>
    {
        public UpdateSupplierRequestValidator()
        {
            RuleFor(request => request.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name must be at most 100 characters");

            RuleFor(request => request.ContactName)
                .MaximumLength(100).WithMessage("ContactName must be at most 100 characters");

            RuleFor(request => request.Phone)
                .MaximumLength(30).WithMessage("Phone must be at most 30 characters");

            RuleFor(request => request.Email)
                .MaximumLength(100).WithMessage("Email must be at most 100 characters")
                .EmailAddress().WithMessage("Email is invalid")
                .When(request => !string.IsNullOrEmpty(request.Email));

            RuleFor(request => request.Note)
                .MaximumLength(200).WithMessage("Note must be at most 200 characters");

            RuleFor(request => request.Status)
                .IsInEnum().WithMessage("Status is invalid");
        }
    }
}
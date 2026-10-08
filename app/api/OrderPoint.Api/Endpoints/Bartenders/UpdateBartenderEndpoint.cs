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

internal sealed record UpdateBartenderRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    BartenderStatus Status,
    string? Notes,
    string? ImageUrl);

internal sealed class UpdateBartenderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/bartenders/{id:guid}", HandleAsync)
            .WithName("UpdateBartender")
            .WithTags("Bartenders");
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
            request.Notes,
            request.ImageUrl);

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

            RuleFor(request => request.ImageUrl)
                .MaximumLength(200).WithMessage("ImageUrl must be at most 200 characters");
        }
    }
}
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

internal sealed record CreateBartenderRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    BartenderStatus Status,
    string? Notes);

internal sealed record CreateBartenderResponse(BartenderDto Data);

internal sealed class CreateBartenderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPost("api/bartenders", HandleAsync)
            .WithName("CreateBartender")
            .WithTags("Bartenders");
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

            RuleFor(request => request.PhoneNumber)
                .MaximumLength(20).WithMessage("PhoneNumber must be at most 20 characters");

            RuleFor(request => request.Status)
                .IsInEnum().WithMessage("Status is invalid");

            RuleFor(request => request.Notes)
                .MaximumLength(500).WithMessage("Notes must be at most 500 characters");
        }
    }
}
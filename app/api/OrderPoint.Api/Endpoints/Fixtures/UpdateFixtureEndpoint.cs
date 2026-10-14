using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Fixtures;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Fixtures;

/// <summary>The fixture's new values.</summary>
/// <param name="Type">What it is: `BarCounter`, `Entrance` or `Wall`.</param>
/// <param name="Label">An optional label shown on it.</param>
internal sealed record UpdateFixtureRequest(FixtureType Type, string? Label);

internal sealed class UpdateFixtureEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/fixtures/{id:guid}", HandleAsync)
            .WithName("UpdateFixture")
            .WithTags("Fixtures")
            .WithSummary("Update a fixture")
            .WithDescription("""
                Updates the type and label; `PUT api/fixtures/{id}/position` moves it.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateFixtureRequest request,
        [FromServices] IValidator<UpdateFixtureRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        UpdateFixtureCommand command = new(id, request.Type, request.Label);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class UpdateFixtureRequestValidator : AbstractValidator<UpdateFixtureRequest>
    {
        public UpdateFixtureRequestValidator()
        {
            RuleFor(request => request.Type)
                .IsInEnum().WithMessage("Type is invalid");

            RuleFor(request => request.Label)
                .MaximumLength(30).WithMessage("Label must be at most 30 characters");
        }
    }
}
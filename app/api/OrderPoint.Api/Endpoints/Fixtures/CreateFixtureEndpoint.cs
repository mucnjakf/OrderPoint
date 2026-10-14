using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Fixtures;
using OrderPoint.Application.Dtos;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Fixtures;

/// <summary>The new fixture and its place on the floor plan.</summary>
/// <param name="AreaId">The id of the area to put it in.</param>
/// <param name="Type">What it is: `BarCounter`, `Entrance` or `Wall`.</param>
/// <param name="Label">An optional label shown on it.</param>
/// <param name="X">The left edge on the grid, in cells from 0.</param>
/// <param name="Y">The top edge on the grid, in cells from 0.</param>
/// <param name="Width">The width, in cells.</param>
/// <param name="Height">The height, in cells.</param>
internal sealed record CreateFixtureRequest(
    Guid AreaId,
    FixtureType Type,
    string? Label,
    int X,
    int Y,
    int Width,
    int Height);

/// <summary>The created fixture.</summary>
/// <param name="Data">The fixture.</param>
internal sealed record CreateFixtureResponse(FixtureDto Data);

internal sealed class CreateFixtureEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPost("api/fixtures", HandleAsync)
            .WithName("CreateFixture")
            .WithTags("Fixtures")
            .WithSummary("Create a fixture")
            .WithDescription("""
                Creates a fixed piece on an area's floor plan. It must lie completely inside the area's grid.

                - `400 Fixture.OutsideArea`: the fixture would lie outside the area's grid
                - `404 Area.NotFound`: the area does not exist
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<CreatedAtRoute<CreateFixtureResponse>, ProblemHttpResult>> HandleAsync(
        [FromBody] CreateFixtureRequest request,
        [FromServices] IValidator<CreateFixtureRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        CreateFixtureCommand command = new(
            request.AreaId,
            request.Type,
            request.Label,
            request.X,
            request.Y,
            request.Width,
            request.Height);

        Result<FixtureDto> result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(
                new CreateFixtureResponse(result.Value),
                "GetFixture",
                new { id = result.Value.Id })
            : result.ToProblemDetails();
    }

    internal sealed class CreateFixtureRequestValidator : AbstractValidator<CreateFixtureRequest>
    {
        public CreateFixtureRequestValidator()
        {
            RuleFor(request => request.AreaId)
                .NotEmpty().WithMessage("AreaId is required");

            RuleFor(request => request.Type)
                .IsInEnum().WithMessage("Type is invalid");

            RuleFor(request => request.Label)
                .MaximumLength(30).WithMessage("Label must be at most 30 characters");

            RuleFor(request => request.X)
                .GreaterThanOrEqualTo(0).WithMessage("X must not be negative");

            RuleFor(request => request.Y)
                .GreaterThanOrEqualTo(0).WithMessage("Y must not be negative");

            RuleFor(request => request.Width)
                .InclusiveBetween(1, 48).WithMessage("Width must be between 1 and 48");

            RuleFor(request => request.Height)
                .InclusiveBetween(1, 48).WithMessage("Height must be between 1 and 48");
        }
    }
}
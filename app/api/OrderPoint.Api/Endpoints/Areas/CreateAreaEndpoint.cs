using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Areas;
using OrderPoint.Application.Dtos;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Areas;

/// <summary>The new area.</summary>
/// <param name="Name">The name, e.g. `Terrace`; unique.</param>
/// <param name="Columns">The width of the floor plan grid, in cells.</param>
/// <param name="Rows">The height of the floor plan grid, in cells.</param>
internal sealed record CreateAreaRequest(string Name, int Columns, int Rows);

/// <summary>The created area.</summary>
/// <param name="Data">The area, still without tables and fixtures.</param>
internal sealed record CreateAreaResponse(AreaDto Data);

internal sealed class CreateAreaEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPost("api/areas", HandleAsync)
            .WithName("CreateArea")
            .WithTags("Areas")
            .WithSummary("Create an area")
            .WithDescription("""
                Creates an area with an empty floor plan, after the existing ones (`displayOrder` is the highest so
                far plus one).

                - `409 Area.NameAlreadyExists`: another area has this name
                """)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<CreatedAtRoute<CreateAreaResponse>, ProblemHttpResult>> HandleAsync(
        [FromBody] CreateAreaRequest request,
        [FromServices] IValidator<CreateAreaRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        CreateAreaCommand command = new(request.Name, request.Columns, request.Rows);

        Result<AreaDto> result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(
                new CreateAreaResponse(result.Value),
                "GetArea",
                new { id = result.Value.Id })
            : result.ToProblemDetails();
    }

    internal sealed class CreateAreaRequestValidator : AbstractValidator<CreateAreaRequest>
    {
        public CreateAreaRequestValidator()
        {
            RuleFor(request => request.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(30).WithMessage("Name must be at most 30 characters");

            RuleFor(request => request.Columns)
                .InclusiveBetween(4, 48).WithMessage("Columns must be between 4 and 48");

            RuleFor(request => request.Rows)
                .InclusiveBetween(4, 48).WithMessage("Rows must be between 4 and 48");
        }
    }
}
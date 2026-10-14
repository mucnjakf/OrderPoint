using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Tables;
using OrderPoint.Application.Dtos;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Tables;

/// <summary>The new table and its place on the floor plan.</summary>
/// <param name="Code">The code guests order with, e.g. `T7`: capital letters, digits and hyphens; unique.</param>
/// <param name="AreaId">The id of the area to put it in.</param>
/// <param name="Seats">The number of seats.</param>
/// <param name="Status">The status. An inactive table takes no orders.</param>
/// <param name="Shape">The shape drawn on the floor plan.</param>
/// <param name="X">The left edge on the grid, in cells from 0.</param>
/// <param name="Y">The top edge on the grid, in cells from 0.</param>
/// <param name="Width">The width, in cells.</param>
/// <param name="Height">The height, in cells.</param>
internal sealed record CreateTableRequest(
    string Code,
    Guid AreaId,
    int Seats,
    TableStatus Status,
    TableShape Shape,
    int X,
    int Y,
    int Width,
    int Height);

/// <summary>The created table.</summary>
/// <param name="Data">The table.</param>
internal sealed record CreateTableResponse(TableDto Data);

internal sealed class CreateTableEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPost("api/tables", HandleAsync)
            .WithName("CreateTable")
            .WithTags("Tables")
            .WithSummary("Create a table")
            .WithDescription("""
                Creates a table on an area's floor plan. It must lie completely inside the area's grid; the API does
                not check overlaps, the floor plan editor does.

                - `400 Table.OutsideArea`: the table would lie outside the area's grid
                - `404 Area.NotFound`: the area does not exist
                - `409 Table.CodeAlreadyExists`: another table has this code
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<CreatedAtRoute<CreateTableResponse>, ProblemHttpResult>> HandleAsync(
        [FromBody] CreateTableRequest request,
        [FromServices] IValidator<CreateTableRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        CreateTableCommand command = new(
            request.Code,
            request.AreaId,
            request.Seats,
            request.Status,
            request.Shape,
            request.X,
            request.Y,
            request.Width,
            request.Height);

        Result<TableDto> result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(
                new CreateTableResponse(result.Value),
                "GetTable",
                new { id = result.Value.Id })
            : result.ToProblemDetails();
    }

    internal sealed class CreateTableRequestValidator : AbstractValidator<CreateTableRequest>
    {
        public CreateTableRequestValidator()
        {
            RuleFor(request => request.Code)
                .NotEmpty().WithMessage("Code is required")
                .MaximumLength(10).WithMessage("Code must be at most 10 characters")
                .Matches("^[A-Z0-9-]+$").WithMessage("Code may only contain capital letters, digits and hyphens");

            RuleFor(request => request.AreaId)
                .NotEmpty().WithMessage("AreaId is required");

            RuleFor(request => request.Seats)
                .InclusiveBetween(1, 20).WithMessage("Seats must be between 1 and 20");

            RuleFor(request => request.Status)
                .IsInEnum().WithMessage("Status is invalid");

            RuleFor(request => request.Shape)
                .IsInEnum().WithMessage("Shape is invalid");

            RuleFor(request => request.X)
                .GreaterThanOrEqualTo(0).WithMessage("X must not be negative");

            RuleFor(request => request.Y)
                .GreaterThanOrEqualTo(0).WithMessage("Y must not be negative");

            RuleFor(request => request.Width)
                .InclusiveBetween(1, 12).WithMessage("Width must be between 1 and 12");

            RuleFor(request => request.Height)
                .InclusiveBetween(1, 12).WithMessage("Height must be between 1 and 12");
        }
    }
}
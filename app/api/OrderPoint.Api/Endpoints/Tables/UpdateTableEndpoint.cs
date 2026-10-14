using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Tables;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Tables;

/// <summary>The table's new values.</summary>
/// <param name="Code">The code guests order with, e.g. `T7`: capital letters, digits and hyphens; unique.</param>
/// <param name="Seats">The number of seats.</param>
/// <param name="Status">The status. An inactive table takes no orders.</param>
/// <param name="Shape">The shape drawn on the floor plan.</param>
internal sealed record UpdateTableRequest(
    string Code,
    int Seats,
    TableStatus Status,
    TableShape Shape);

internal sealed class UpdateTableEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/tables/{id:guid}", HandleAsync)
            .WithName("UpdateTable")
            .WithTags("Tables")
            .WithSummary("Update a table")
            .WithDescription("""
                Updates the code, seats, status and shape; `PUT api/tables/{id}/position` moves it. A new code makes
                the table's printed QR code invalid. Past orders keep the code they were placed with.

                - `409 Table.CodeAlreadyExists`: another table has this code
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateTableRequest request,
        [FromServices] IValidator<UpdateTableRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        UpdateTableCommand command = new(id, request.Code, request.Seats, request.Status, request.Shape);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class UpdateTableRequestValidator : AbstractValidator<UpdateTableRequest>
    {
        public UpdateTableRequestValidator()
        {
            RuleFor(request => request.Code)
                .NotEmpty().WithMessage("Code is required")
                .MaximumLength(10).WithMessage("Code must be at most 10 characters")
                .Matches("^[A-Z0-9-]+$").WithMessage("Code may only contain capital letters, digits and hyphens");

            RuleFor(request => request.Seats)
                .InclusiveBetween(1, 20).WithMessage("Seats must be between 1 and 20");

            RuleFor(request => request.Status)
                .IsInEnum().WithMessage("Status is invalid");

            RuleFor(request => request.Shape)
                .IsInEnum().WithMessage("Shape is invalid");
        }
    }
}
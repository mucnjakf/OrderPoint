using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.TableRequests;
using OrderPoint.Application.Dtos;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.TableRequests;

/// <summary>What the guest asks for.</summary>
/// <param name="Type">`CallBartender` or `Bill`.</param>
internal sealed record CreateTableRequestRequest(TableRequestType Type);

/// <summary>The open request.</summary>
/// <param name="Data">The request, new or the one already open.</param>
internal sealed record CreateTableRequestResponse(TableRequestDto Data);

internal sealed class CreateTableRequestEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPost("api/tables/by-code/{code}/requests", HandleAsync)
            .WithName("CreateTableRequest")
            .WithTags("Table requests")
            .WithSummary("Call a bartender or ask for the bill")
            .WithDescription("""
                A guest at the table asks for a bartender or the bill; the bar app shows it until a bartender
                marks it done. No token is needed. A table has at most one open request of each type: asking again
                returns the open one, so this is `200`, not `201`.

                - `404 Table.NotFound`: no table has this code
                - `409 Table.IsInactive`: the table is closed
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<Results<Ok<CreateTableRequestResponse>, ProblemHttpResult>> HandleAsync(
        [FromRoute] string code,
        [FromBody] CreateTableRequestRequest request,
        [FromServices] IValidator<CreateTableRequestRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        CreateTableRequestCommand command = new(code, request.Type);

        Result<TableRequestDto> result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new CreateTableRequestResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class CreateTableRequestRequestValidator : AbstractValidator<CreateTableRequestRequest>
    {
        public CreateTableRequestRequestValidator()
        {
            RuleFor(request => request.Type)
                .IsInEnum().WithMessage("Type is invalid");
        }
    }
}
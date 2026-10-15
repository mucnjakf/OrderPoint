using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Stock;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Stock;

/// <summary>Paging options.</summary>
/// <param name="PageNumber">The page to return, starting at 1.</param>
/// <param name="PageSize">The number of movements per page.</param>
internal sealed record GetStockMovementsRequest(
    [FromQuery] int PageNumber,
    [FromQuery] int PageSize);

/// <summary>A page of a stock item's movements.</summary>
/// <param name="Data">The page, the latest first.</param>
internal sealed record GetStockMovementsResponse(PaginationDto<StockMovementDto> Data);

internal sealed class GetStockMovementsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/stock-items/{id:guid}/movements", HandleAsync)
            .WithName("GetStockMovements")
            .WithTags("Stock")
            .WithSummary("List a stock item's movements")
            .WithDescription("""
                The stock item's history, the latest first: every change to its quantity with the quantity after it
                and what it was worth at the cost of its time.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<GetStockMovementsResponse>, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [AsParameters] GetStockMovementsRequest request,
        [FromServices] IValidator<GetStockMovementsRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetStockMovementsQuery query = new(id, request.PageNumber, request.PageSize);

        Result<PaginationDto<StockMovementDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetStockMovementsResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class GetStockMovementsRequestValidator : AbstractValidator<GetStockMovementsRequest>
    {
        public GetStockMovementsRequestValidator()
        {
            RuleFor(request => request.PageNumber)
                .GreaterThan(0).WithMessage("PageNumber must be positive");

            RuleFor(request => request.PageSize)
                .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100");
        }
    }
}
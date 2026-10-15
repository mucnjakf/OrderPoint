using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Deliveries;
using OrderPoint.Domain.Outcomes;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Api.Endpoints.Deliveries;

/// <summary>Paging, search, filter and sort options.</summary>
/// <param name="PageNumber">The page to return, starting at 1.</param>
/// <param name="PageSize">The number of deliveries per page.</param>
/// <param name="SearchQuery">Matches the reference or the supplier's name, ignoring case.</param>
/// <param name="SupplierId">Only deliveries from this supplier.</param>
/// <param name="SortBy">The sort order. Default: `DeliveredOnDesc` (the latest first).</param>
internal sealed record GetDeliveriesRequest(
    [FromQuery] int PageNumber,
    [FromQuery] int PageSize,
    [FromQuery] string? SearchQuery,
    [FromQuery] Guid? SupplierId,
    [FromQuery] DeliverySortBy? SortBy);

/// <summary>A page of deliveries.</summary>
/// <param name="Data">The page.</param>
internal sealed record GetDeliveriesResponse(PaginationDto<DeliveryDto> Data);

internal sealed class GetDeliveriesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/deliveries", HandleAsync)
            .WithName("GetDeliveries")
            .WithTags("Deliveries")
            .WithSummary("List deliveries")
            .WithDescription("""
                A page of deliveries with their lines, search (reference or supplier), a supplier filter and sorting
                (default: the latest first).
                """)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<GetDeliveriesResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetDeliveriesRequest request,
        [FromServices] IValidator<GetDeliveriesRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetDeliveriesQuery query = new(
            request.PageNumber,
            request.PageSize,
            request.SearchQuery,
            request.SupplierId,
            request.SortBy);

        Result<PaginationDto<DeliveryDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetDeliveriesResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class GetDeliveriesRequestValidator : AbstractValidator<GetDeliveriesRequest>
    {
        public GetDeliveriesRequestValidator()
        {
            RuleFor(request => request.PageNumber)
                .GreaterThan(0).WithMessage("PageNumber must be positive");

            RuleFor(request => request.PageSize)
                .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100");

            RuleFor(request => request.SearchQuery)
                .MaximumLength(100).WithMessage("SearchQuery must be at most 100 characters");

            RuleFor(request => request.SortBy)
                .IsInEnum().WithMessage("SortBy is invalid");
        }
    }
}
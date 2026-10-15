using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Orders;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Api.Endpoints.Orders;

/// <summary>Paging, search, filter and sort options.</summary>
/// <param name="PageNumber">The page to return, starting at 1.</param>
/// <param name="PageSize">The number of orders per page.</param>
/// <param name="SearchQuery">Matches the order number, the table code or a counter order's customer name.</param>
/// <param name="Status">Only orders with this status.</param>
/// <param name="ItemId">Only orders that contain this item.</param>
/// <param name="BartenderId">Only orders handled by this bartender.</param>
/// <param name="TableId">Only orders placed from this table.</param>
/// <param name="TakenByBartenderId">Only orders this bartender took in the Bartender app.</param>
/// <param name="PaymentStatus">Only orders with this payment status, e.g. `Unpaid` for the register.</param>
/// <param name="SortBy">The sort order. Default: `CreatedAtUtcDesc` (newest first).</param>
internal sealed record GetOrdersRequest(
    [FromQuery] int PageNumber,
    [FromQuery] int PageSize,
    [FromQuery] string? SearchQuery,
    [FromQuery] OrderStatus? Status,
    [FromQuery] Guid? ItemId,
    [FromQuery] Guid? BartenderId,
    [FromQuery] Guid? TableId,
    [FromQuery] Guid? TakenByBartenderId,
    [FromQuery] PaymentStatus? PaymentStatus,
    [FromQuery] OrderSortBy? SortBy);

/// <summary>A page of orders.</summary>
/// <param name="Data">The page.</param>
internal sealed record GetOrdersResponse(PaginationDto<OrderDto> Data);

internal sealed class GetOrdersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/orders", HandleAsync)
            .WithName("GetOrders")
            .WithTags("Orders")
            .WithSummary("List orders")
            .WithDescription("""
                A page of orders with search (order number, table code or customer name), filters (status, item,
                bartender, table, the bartender who took it, payment status) and sorting (default: newest first).
                `total` is calculated from the order lines.
                """)
            .RequireAuthorization(AuthorizationPolicies.AdminOrBartender);
    }

    private static async Task<Results<Ok<GetOrdersResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetOrdersRequest request,
        [FromServices] IValidator<GetOrdersRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetOrdersQuery query = new(
            request.PageNumber,
            request.PageSize,
            request.SearchQuery,
            request.Status,
            request.ItemId,
            request.BartenderId,
            request.TableId,
            request.TakenByBartenderId,
            request.PaymentStatus,
            request.SortBy);

        Result<PaginationDto<OrderDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetOrdersResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class GetOrdersRequestValidator : AbstractValidator<GetOrdersRequest>
    {
        public GetOrdersRequestValidator()
        {
            RuleFor(request => request.PageNumber)
                .GreaterThan(0).WithMessage("PageNumber must be positive");

            RuleFor(request => request.PageSize)
                .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100");

            RuleFor(request => request.SearchQuery)
                .MaximumLength(100).WithMessage("SearchQuery must be at most 100 characters");

            RuleFor(request => request.Status)
                .IsInEnum().WithMessage("Status is invalid");

            RuleFor(request => request.SortBy)
                .IsInEnum().WithMessage("SortBy is invalid");
        }
    }
}
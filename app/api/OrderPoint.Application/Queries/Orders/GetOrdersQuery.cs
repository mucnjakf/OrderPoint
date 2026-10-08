using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Application.Queries.Orders;

public sealed record GetOrdersQuery(
    int PageNumber,
    int PageSize,
    string? SearchQuery,
    OrderStatus? Status,
    Guid? ItemId,
    OrderSortBy? SortBy)
    : IQuery<PaginationDto<OrderDto>>;

internal sealed class GetOrdersQueryHandler(IOrderRepository orderRepository)
    : IQueryHandler<GetOrdersQuery, PaginationDto<OrderDto>>
{
    public async Task<Result<PaginationDto<OrderDto>>> Handle(
        GetOrdersQuery query,
        CancellationToken cancellationToken)
    {
        (IReadOnlyList<Order> orders, int totalCount) = await orderRepository
            .GetPaginatedAsync(
                query.PageNumber,
                query.PageSize,
                query.SearchQuery,
                query.Status,
                query.ItemId,
                query.SortBy,
                cancellationToken);

        return new PaginationDto<OrderDto>(
            orders.Select(order => order.ToOrderDto()).ToList(),
            query.PageNumber,
            query.PageSize,
            totalCount);
    }
}
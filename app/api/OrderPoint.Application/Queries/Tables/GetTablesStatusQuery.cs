using OrderPoint.Application.Dtos;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Tables;

// Not cached: it changes with every order, and the bar app reloads it on each OrdersChanged
public sealed record GetTablesStatusQuery : IQuery<IReadOnlyList<TableStatusDto>>;

internal sealed class GetTablesStatusQueryHandler(IOrderRepository orderRepository)
    : IQueryHandler<GetTablesStatusQuery, IReadOnlyList<TableStatusDto>>
{
    private static readonly IReadOnlyList<OrderStatus> OpenStatuses =
        [OrderStatus.Pending, OrderStatus.Accepted, OrderStatus.Active];

    // Only tables with open orders; the others are quiet
    public async Task<Result<IReadOnlyList<TableStatusDto>>> Handle(
        GetTablesStatusQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Order> openOrders = await orderRepository.GetByStatusesAsync(OpenStatuses, cancellationToken);

        List<TableStatusDto> tables = openOrders
            .GroupBy(order => order.TableId)
            .Select(group => new TableStatusDto(
                group.Key,
                group.Count(order => order.Status == OrderStatus.Pending),
                group.Count(order => order.Status != OrderStatus.Pending),
                group.Min(order => order.CreatedAtUtc)))
            .ToList();

        return Result.Success<IReadOnlyList<TableStatusDto>>(tables);
    }
}
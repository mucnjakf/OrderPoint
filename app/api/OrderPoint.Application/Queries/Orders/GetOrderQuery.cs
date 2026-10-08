using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Orders;

public sealed record GetOrderQuery(Guid Id) : IQuery<OrderDto>;

internal sealed class GetOrderQueryHandler(IOrderRepository orderRepository)
    : IQueryHandler<GetOrderQuery, OrderDto>
{
    public async Task<Result<OrderDto>> Handle(GetOrderQuery query, CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.GetAsync(query.Id, cancellationToken);

        if (order is null)
        {
            return Result.Failure<OrderDto>(OrderErrors.NotFound);
        }

        var orderDto = order.ToOrderDto();

        return Result.Success(orderDto);
    }
}
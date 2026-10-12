using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Orders;

public sealed record GetOrderQuery(Guid Id) : IQuery<OrderDto>;

internal sealed class GetOrderQueryHandler(IOrderRepository orderRepository, HybridCache cache)
    : IQueryHandler<GetOrderQuery, OrderDto>
{
    public async Task<Result<OrderDto>> Handle(GetOrderQuery query, CancellationToken cancellationToken)
    {
        OrderDto? orderDto = await cache.GetOrCreateAsync(
            CacheKeys.For(query),
            query,
            GetOrderAsync,
            tags: [CacheTags.Orders],
            cancellationToken: cancellationToken);

        if (orderDto is null)
        {
            return Result.Failure<OrderDto>(OrderErrors.NotFound);
        }

        return Result.Success(orderDto);
    }

    private async ValueTask<OrderDto?> GetOrderAsync(GetOrderQuery query, CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.GetAsync(query.Id, cancellationToken);

        if (order is null)
        {
            return null;
        }

        return order.ToOrderDto();
    }
}